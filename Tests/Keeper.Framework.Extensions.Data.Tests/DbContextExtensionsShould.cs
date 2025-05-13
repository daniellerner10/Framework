using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Keeper.Framework.Extensions.Collections;
using Npgsql;
using Microsoft.EntityFrameworkCore.Storage;


namespace Keeper.Framework.Extensions.Data.Tests
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "It is necessary")]
    [SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped", Justification = "We need to skip this until we figure out how to mock it.")]
    public class DbContextExtensionsShould
    {
        internal const string ConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=password;Database=test;Include Error Detail=True;MaxPoolSize=600;ConnectionIdleLifetime=600;ConnectionLifetime=1200;TcpKeepalive=true;Keepalive=30;";

        internal static readonly Action<DbContextOptionsBuilder> UseDatabaseActionPostgres = x => x.UseNpgsql(ConnectionString);
        internal static readonly Action<DbContextOptionsBuilder> UseDatabaseActionSqlite = x => x.UseSqlite($"Data Source={nameof(DbContextExtensionsShould)}.db");
        internal static readonly Action<DbContextOptionsBuilder> UseDatabaseAction = UseDatabaseActionPostgres;

        [Fact(Skip = "Can not run unit test in pipeline. skip until we figure out how to mock this.")]
        public async Task BulkInsertCorrectly()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(UseDatabaseAction);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<TestDbContext>();
            context.Database.Migrate();

            var list = new List<TestPerson> {
                new() { Id = 1, FirstName = "Nati", LastName = "Livni", CreatedDate = DateTime.Parse("1976-01-01"), IsActive = true, City = "NA", OptIn = null, UpdatedDate = null, Version = null },
                new() { Id = 2, FirstName = "Daniel", LastName = "Lerner", CreatedDate = DateTime.Parse("1981-01-01"), IsActive = true, City = "Jerusalem", OptIn = true, UpdatedDate = DateTime.Parse("2000-01-01"), Version = 1  },
                new() { Id = 3, FirstName = "Donny", LastName = "Weltman", CreatedDate = DateTime.Parse("1980-01-01"), IsActive = true, City = "Yad Binyamin", OptIn = false, UpdatedDate = null, Version = 2 },
            };

            await context.BulkInsertAsync(list);
        }

        [Fact] //(Skip = "Can not run unit test in pipeline. skip until we figure out how to mock this.")]
        public async Task GetMergeSqlWithUpdateAndInsertNoConditionsAndJsonMethodsCorrectly()
        {
            var context = GetContext();

            var input = new List<TestPersonInfoThin>
               {
                    new() { Id = 1, Age = 10 },
                    new() { Id = 2, Age = 20 },
                    new() { Id = 3, Age = 30 }
               }.ToAsyncEnumerable();

            await context.Merge(
                x => x.TestAges,
                input,
                (u, s) => u.Id == s.Id,
                actions => actions
                    .Insert(new()
                    {
                        [u => u.Id] = (u, s) => s.Id,
                        [u => u.Age] = (u, s) => s.Age,
                    })
                    .Update(new()
                    {
                        [u => u.Age] = (u, s) => s.Age,
                        [u => u.History] = (u, s) =>
                             EF.Functions.Coalesce(u.History, EF.Functions.JsonbBuildArray()) +
                             EF.Functions.JsonbBuildArray(
                                 EF.Functions.JsonbBuildObject(
                                     "when", DateTime.Now,
                                     "age", s.Age
                                 )
                             )
                    }),
                CancellationToken.None
            );

            var connection = context.Connection;

            Assert.NotNull(connection);
            Assert.Equal(ConnectionState.Open, connection.State);

            var transaction = Assert.Single(connection.Transactions);
            Assert.True(transaction.IsCommited);
            Assert.True(transaction.IsDisposed);

            Assert.Collection(connection.Commands,
                tempTableCommand => Assert.Equal(
                    """
                    CREATE TEMP TABLE testpersoninfothin_aaaaaaaa
                    ("Id" integer, "Age" integer)
                    ON COMMIT DROP
                    """,
                    tempTableCommand.CommandText
                ),
                mergeCommand => Assert.Equal(
                    """
                    MERGE INTO "TestAges" u
                    USING testpersoninfothin_aaaaaaaa s
                    ON u."Id" = s."Id"
                    WHEN NOT MATCHED THEN
                      INSERT ("Id", "Age")
                      VALUES (s."Id", s."Age")
                    WHEN MATCHED THEN
                      UPDATE SET
                        "Age" = s."Age"
                        , "History" = coalesce(u."History", jsonb_build_array()) || jsonb_build_array(jsonb_build_object('when', now(), 'age', s."Age"))
                    
                    """,
                    mergeCommand.CommandText
                )
            );
        }

        [Fact] //(Skip = "Can not run unit test in pipeline. skip until we figure out how to mock this.")]
        public async Task GetMergeSqlWithUpdateAndInsertWithConditionsCorrectly()
        {
            var context = GetContext();

            var input = new List<TestPersonInfoThin>
               {
                    new() { Id = 1, Age = 10 },
                    new() { Id = 2, Age = 20 },
                    new() { Id = 3, Age = 110 }
               }.ToAsyncEnumerable();

            await context.Merge(
                x => x.TestAges,
                input,
                (u, s) => u.Id == s.Id,
                actions => actions
                    .DoNothing(
                        (u, s) => s.Age > 100,
                        MatchType.OnNotMatch
                    )
                    .DoNothing(
                        (u, s) => s.Age > 100,
                        MatchType.OnMatch
                    )
                    .Insert(
                        (u, s) => s.Age < 18,
                        new()
                        {
                            [u => u.Id] = (u, s) => s.Id,
                            [u => u.Age] = (u, s) => s.Age,
                            [u => u.Category] = (u, s) => "Child"
                        }
                    )
                    .Insert(
                        (u, s) => s.Age >= 18,
                        new()
                        {
                            [u => u.Id] = (u, s) => s.Id,
                            [u => u.Age] = (u, s) => s.Age,
                            [u => u.Category] = (u, s) => "Adult"
                        }
                    )
                    .Update(
                        (u, s) => s.Age < 18,
                        new()
                        {
                            [u => u.Age] = (u, s) => s.Age,
                            [u => u.Category] = (u, s) => "Child"
                        }
                    )
                    .Update(
                        (u, s) => s.Age >= 18,
                        new()
                        {
                            [u => u.Age] = (u, s) => s.Age,
                            [u => u.Category] = (u, s) => "Adult"
                        }
                    ),
                CancellationToken.None
            );

            var connection = context.Connection;

            Assert.NotNull(connection);
            Assert.Equal(ConnectionState.Open, connection.State);

            var transaction = Assert.Single(connection.Transactions);
            Assert.True(transaction.IsCommited);
            Assert.True(transaction.IsDisposed);

            Assert.Collection(connection.Commands,
                tempTableCommand => Assert.Equal(
                    """
                    CREATE TEMP TABLE testpersoninfothin_aaaaaaaa
                    ("Id" integer, "Age" integer)
                    ON COMMIT DROP
                    """,
                    tempTableCommand.CommandText
                ),
                mergeCommand => Assert.Equal(
                    """
                    MERGE INTO "TestAges" u
                    USING testpersoninfothin_aaaaaaaa s
                    ON u."Id" = s."Id"
                    WHEN NOT MATCHED AND s."Age" > 100 THEN
                      DO NOTHING
                    WHEN MATCHED AND s."Age" > 100 THEN
                      DO NOTHING
                    WHEN NOT MATCHED AND s."Age" < 18 THEN
                      INSERT ("Id", "Age", "Category")
                      VALUES (s."Id", s."Age", 'Child')
                    WHEN NOT MATCHED AND s."Age" >= 18 THEN
                      INSERT ("Id", "Age", "Category")
                      VALUES (s."Id", s."Age", 'Adult')
                    WHEN MATCHED AND s."Age" < 18 THEN
                      UPDATE SET
                        "Age" = s."Age"
                        , "Category" = 'Child'
                    WHEN MATCHED AND s."Age" >= 18 THEN
                      UPDATE SET
                        "Age" = s."Age"
                        , "Category" = 'Adult'

                    """,
                    mergeCommand.CommandText
                )
            );
        }


        [Fact]
        public async Task GetMergeSqlWithUpdateAndInsertNoConditionsWithReturnValueCorrectly()
        {
            var context = GetContext();

            var input = new List<TestPersonInfoThin>
               {
                    new() { Id = 1, Age = 10 },
                    new() { Id = 2, Age = 20 },
                    new() { Id = 3, Age = 30 }
               }.ToAsyncEnumerable();

            var itemEnum = await context.Merge(
               x => x.TestAges,
               input,
               (u, s) => u.Id == s.Id,
               actions => actions
                   .Insert(new()
                   {
                       [u => u.Id] = (u, s) => s.Id,
                       [u => u.Age] = (u, s) => s.Age,
                   })
                   .Update(new()
                   {
                       [u => u.Age] = (u, s) => s.Age,
                       [u => u.History] = (u, s) =>
                            EF.Functions.Coalesce(u.History, EF.Functions.JsonbBuildArray()) +
                            EF.Functions.JsonbBuildArray(
                                EF.Functions.JsonbBuildObject(
                                    "when", DateTime.Now,
                                    "age", s.Age
                                )
                            )
                   }),
               (u, s) => new ReturnType
               {
                   Type = u.GetMergeType(),
                   Num = u.Id,
                   Years = u.Age,
               },
               CancellationToken.None
           ).ToListAsync();

            var connection = context.Connection;

            Assert.NotNull(connection);
            Assert.Equal(ConnectionState.Open, connection.State);

            var transaction = Assert.Single(connection.Transactions);
            Assert.True(transaction.IsCommited);
            Assert.True(transaction.IsDisposed);

            Assert.Collection(connection.Commands,
                tempTableCommand => Assert.Equal(
                    """
                    CREATE TEMP TABLE testpersoninfothin_aaaaaaaa
                    ("Id" integer, "Age" integer)
                    ON COMMIT DROP
                    """,
                    tempTableCommand.CommandText
                ),
                mergeCommand => Assert.Equal(
                    """
                    MERGE INTO "TestAges" u
                    USING testpersoninfothin_aaaaaaaa s
                    ON u."Id" = s."Id"
                    WHEN NOT MATCHED THEN
                      INSERT ("Id", "Age")
                      VALUES (s."Id", s."Age")
                    WHEN MATCHED THEN
                      UPDATE SET
                        "Age" = s."Age"
                        , "History" = coalesce(u."History", jsonb_build_array()) || jsonb_build_array(jsonb_build_object('when', now(), 'age', s."Age"))

                    RETURNING merge_action() "Type", u."Id" "Num", u."Age" "Years"
                    """,
                    mergeCommand.CommandText
                )
            );

            Assert.Collection(itemEnum,
                row =>
                {
                    Assert.Equal(MergeType.Update, row.Type);
                    Assert.Equal(1, row.Num);
                    Assert.Equal(10, row.Years);
                },
                row =>
                {
                    Assert.Equal(MergeType.Insert, row.Type);
                    Assert.Equal(2, row.Num);
                    Assert.Equal(20, row.Years);
                },
                row =>
                {
                    Assert.Equal(MergeType.Insert, row.Type);
                    Assert.Equal(3, row.Num);
                    Assert.Equal(30, row.Years);
                }
            );
        }

        private static TestDbContext GetContext()
        {
            DbContextExtensions.GetConnection = c =>
            {
                if (c is TestDbContext testContext)
                {
                    if (testContext.Connection is null)
                        return testContext.Connection = new MockConnection();
                    else
                        return testContext.Connection;
                }
                else
                    return new MockConnection();
            };

            DbContextExtensions.BeginBinaryImportAsync = (string sql, DbConnection connection, CancellationToken cancellationToken) =>
            {
                if (connection is MockConnection mockConnection)
                    return Task.FromResult(mockConnection.BeginBinaryImportAsync());
                else
                    throw new NotSupportedException("BeginBinaryImportAsync can only be called on a NpgsqlConnection");
            };

            DbContextExtensions.BeginTransaction =
                async (DbContext context, CancellationToken cancellationToken) =>
                    (MockTransaction)await ((MockConnection)DbContextExtensions.GetConnection(context)).BeginTransactionAsync(cancellationToken);

            DbContextExtensions.GetCurrentTransaction =
                (DbContext context) => ((MockConnection)DbContextExtensions.GetConnection(context))
                    .Transactions
                    .Where(x => !x.IsDisposed)
                    .LastOrDefault();

            DbContextExtensions.GetRandomTableSuffix = (i) => new('a', i);

            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(UseDatabaseAction);
            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<TestDbContext>();
            //context.Database.Migrate();

            return context;
        }

        [Fact]
        public void GetEntityValidationErrorsCorrectly()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(UseDatabaseAction);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<TestDbContext>();

            var designTimeModel = context.GetInfrastructure().GetRequiredService<IDesignTimeModel>();
            var model = designTimeModel.Model.FindEntityType(typeof(TestPerson))!;

            var entity = new TestPerson()
            {
                FirstName = "Natiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii",
                LastName = "Livniiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii",
                City = null,
                CreatedDate = DateTime.Now,
            };

            var errors = DbContextExtensions.GetEntityValidationErrors(entity, model);

            Assert.Collection(errors,
                error => Assert.Equal("Property 'City' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' is not nullable.", error),
                error => Assert.Equal("Property 'FirstName' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' has max length of '20'. Value 'Natiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii' has a length of '40'.", error),
                error => Assert.Equal("Property 'LastName' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' has max length of '25'. Value 'Livniiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii' has a length of '54'.", error)
            );
        }

        [Fact]
        public async Task CastToEntityCorrectly()
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(UseDatabaseAction);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<TestDbContext>();

            var designTimeModel = context.GetInfrastructure().GetRequiredService<IDesignTimeModel>();
            var model = designTimeModel.Model.FindEntityType(typeof(TestPerson))!;

            var goodEntity = new TestPerson()
            {
                FirstName = "Nati",
                LastName = "Livni",
                City = "Yad Binyamin",
                CreatedDate = DateTime.Now,
            };

            var badEntity = new TestPerson()
            {
                FirstName = "Natiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii",
                LastName = "Livniiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii",
                City = null,
                CreatedDate = DateTime.Now,
            };

            var entities = new List<TestPerson>
            {
                goodEntity,
                badEntity
            };

            Exception? singleException = null;
            TestPerson? onErrorEntity = null;

            var goodEntities = context.CastToEntity(
                entities.ToAsyncEnumerable(),
                map: x => Task.FromResult(x),
                onError: (x, ex) =>
                {
                    onErrorEntity = x;
                    singleException = ex;
                    return Task.CompletedTask;
                },
                CancellationToken.None
            );

            var single = await goodEntities.SingleAsync();
            Assert.NotNull(single);
            Assert.Equal(goodEntity, single);

            Assert.NotNull(singleException);
            Assert.NotNull(onErrorEntity);

            Assert.Equal(badEntity, onErrorEntity);
            Assert.Equal("""
                Item 2 of list has a validation error(s):
                Property 'City' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' is not nullable.
                Property 'FirstName' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' has max length of '20'. Value 'Natiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii' has a length of '40'.
                Property 'LastName' of type 'Keeper.Framework.Extensions.Data.Tests.TestPerson' has max length of '25'. Value 'Livniiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii' has a length of '54'.
                """.Replace("\r\n", "\n"), singleException.Message);
        }
    }

    class ReturnType
    {
        public MergeType Type { get; set; }

        public int Num { get; set; }

        public int Years { get; set; }
    }

    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        internal MockConnection? Connection { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestPerson>(build =>
            {
                build.Property(x => x.LastName).HasMaxLength(25);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestPerson> TestPersons { get; set; }

        public DbSet<TestAge> TestAges { get; set; }

        public DbSet<TestPersonInfo> TestPersonInfos { get; set; }
    }

    public class StudyDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            DbContextExtensionsShould.UseDatabaseAction(optionsBuilder);

            return new TestDbContext(optionsBuilder.Options);
        }
    }

    public class TestPerson
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        [Required]
        public string? City { get; set; }

        public string? Nickname { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool? OptIn { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? Version { get; set; }

    }

    public class TestAge
    {
        [Key]
        public int Id { get; set; }

        public int Age { get; set; }

        [Column(TypeName = "jsonb")]
        public string? History { get; set; }

        public string? Category { get; set; }
    }

    public class TestPersonInfo
    {
        [Key]
        public int Id { get; set; }

        public required int Age { get; set; }

        public required string Ssn { get; set; }
    }

    public class TestPersonInfoThin
    {
        [Key]
        public int Id { get; set; }

        public required int Age { get; set; }
    }

    class MockReader : DbDataReader
    {
        private readonly IEnumerable<object[]> _rows;
        private readonly IEnumerator<object[]> _enumerator;

        public MockReader(IEnumerable<object[]> rows)
        {
            _rows = rows;
            _enumerator = _rows.GetEnumerator();
        }

        public override object this[int ordinal] => throw new NotImplementedException();

        public override object this[string name] => throw new NotImplementedException();

        public override int Depth => throw new NotImplementedException();

        public override int FieldCount => throw new NotImplementedException();

        public override bool HasRows => throw new NotImplementedException();

        public override bool IsClosed => throw new NotImplementedException();

        public override int RecordsAffected => throw new NotImplementedException();

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetValues(object[] values)
        {
            Array.Copy(_enumerator.Current, values, _enumerator.Current.Length);
            return _enumerator.Current.Length;
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }

        public override bool Read()
        {
            return _enumerator.MoveNext();
        }
    }

    class MockParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _parameters = [];
        private readonly object _syncRoot = new();

        public override int Count => _parameters.Count;

        public override object SyncRoot => _syncRoot;

        public override int Add(object value)
        {
            _parameters.Add((DbParameter)value);

            return Count - 1;
        }

        public override void AddRange(Array values)
        {
            for (var i = 0; i < values.Length; i++)
                _parameters.Add((DbParameter)values.GetValue(i)!);
        }

        public override void Clear()
        {
            _parameters.Clear();
        }

        public override bool Contains(object value)
        {
            return _parameters.Contains((DbParameter)value);
        }

        public override bool Contains(string value)
        {
            return _parameters.Any(x => x.Value?.ToString() == value);
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf((DbParameter)value);
        }

        public override int IndexOf(string parameterName)
        {
            return _parameters.IndexOf(x => x.ParameterName == parameterName);
        }

        public override void Insert(int index, object value)
        {
            _parameters.Insert(index, (DbParameter)value);
        }

        public override void Remove(object value)
        {
            _parameters.Remove((DbParameter)value);
        }

        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            _parameters.RemoveAt(IndexOf(parameterName));
        }

        protected override DbParameter GetParameter(int index)
        {
            return _parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _parameters[IndexOf(parameterName)];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            Insert(index, value);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            Insert(IndexOf(parameterName), value);
        }
    }

    class MockCommand : DbCommand
    {
        [DefaultValue("")]
        [RefreshProperties(RefreshProperties.All)]
        [AllowNull]
        public override string CommandText { get; set; }

        public override int CommandTimeout { get; set; }

        public override CommandType CommandType { get; set; }

        public override bool DesignTimeVisible { get; set; }

        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection? DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection => new MockParameterCollection();

        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            return 0;
        }

        public override object? ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return CommandText switch
            {
                """
                MERGE INTO "TestAges" u
                USING testpersoninfothin_aaaaaaaa s
                ON u."Id" = s."Id"
                WHEN NOT MATCHED THEN
                  INSERT ("Id", "Age")
                  VALUES (s."Id", s."Age")
                WHEN MATCHED THEN
                  UPDATE SET
                    "Age" = s."Age"
                    , "History" = coalesce(u."History", jsonb_build_array()) || jsonb_build_array(jsonb_build_object('when', now(), 'age', s."Age"))

                RETURNING merge_action() "Type", u."Id" "Num", u."Age" "Years"
                """ => new MockReader([
                                        ["UPDATE", 1, 10],
                        ["INSERT", 2, 20],
                        ["INSERT", 3, 30],
                    ]),
                _ => throw new NotSupportedException($"reader for sql not supported: '{CommandText}'"),
            };
        }
    }

    class MockBinaryWriter : IBinaryWriter
    {
        public bool IsCompleted { get; set; }

        public List<object[]> Values { get; } = [];
        public Task WriteRowAsync(CancellationToken cancellationToken, object[] values)
        {
            Values.Add(values);

            return Task.CompletedTask;
        }

        public ValueTask<ulong> CompleteAsync(CancellationToken cancellationToken)
        {
            if (IsCompleted)
                throw new NotSupportedException("Can not complete stop.");

            IsCompleted = true;

            return ValueTask.FromResult((ulong)Values.Count);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    class MockConnection : DbConnection 
    {
        private string? _connectionString = "Test";
        private ConnectionState _connectionState;

        public MockConnection()
        {
        }

        public List<MockTransaction> Transactions { get; } = [];

        public List<MockCommand> Commands { get; } = [];

        public List<MockBinaryWriter> BinaryWriters { get; } = [];

        [DefaultValue("")]
        [SettingsBindable(true)]
        [RefreshProperties(RefreshProperties.All)]
#pragma warning disable 618 // ignore obsolete warning about RecommendedAsConfigurable to use SettingsBindableAttribute
        [RecommendedAsConfigurable(true)]
#pragma warning restore 618
        [AllowNull]
        public override string ConnectionString
        {
            get => _connectionString!;
            set => _connectionString = value;
        }

        public string _database = "Test";
        public override string Database => _database;

        public override string DataSource => "Test";

        public override string ServerVersion => "Test";

        public override ConnectionState State => _connectionState;

        public override void ChangeDatabase(string databaseName)
        {
            _database = databaseName;
        }

        public override void Close()
        {
            if (_connectionState == ConnectionState.Closed)
                throw new NotSupportedException("Can not close connection twice.");

            _connectionState = ConnectionState.Closed;
        }

        public override void Open()
        {
            if (_connectionState == ConnectionState.Open)
                throw new NotSupportedException("Can not open connection twice.");

            _connectionState = ConnectionState.Open;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = new MockTransaction(this);

            Transactions.Add(transaction);

            return transaction;
        }

        protected override DbCommand CreateDbCommand()
        {
            var command = new MockCommand
            {
                Connection = this
            };

            var transaction = Transactions.LastOrDefault();
            if (transaction is not null && !transaction.IsDisposed)
                command.Transaction = transaction;

            Commands.Add(command);

            return command;
        }

        internal IBinaryWriter BeginBinaryImportAsync()
        {
            var binaryWriter = new MockBinaryWriter();

            BinaryWriters.Add(binaryWriter);

            return binaryWriter;
        }
    }

    class MockTransaction(DbConnection _connection) : DbTransaction, IDbContextTransaction
    {
        public Guid TransactionId => Guid.Empty;

        public bool IsCommited { get; set; }

        public bool IsRolledback { get; set; }

        public bool IsDisposed { get; set; }

        public override IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

        protected override DbConnection? DbConnection => _connection;

        public override void Commit()
        {
            if (IsDisposed)
                throw new InvalidOperationException("can not commit disposed transaction.");

            if (IsCommited)
                throw new InvalidOperationException("can not commit twice.");

            if (IsRolledback)
                throw new InvalidOperationException("can not commit rolledback transaction.");

            IsCommited = true;
        }

        public new void Dispose()
        {
            if (IsDisposed)
                throw new InvalidOperationException("already disposed.");

            IsDisposed = true;
        }

        public override ValueTask DisposeAsync()
        {
            Dispose();

            return ValueTask.CompletedTask;
        }

        public override void Rollback()
        {
            if (IsDisposed)
                throw new InvalidOperationException("can not rollback disposed transaction.");

            if (IsCommited)
                throw new InvalidOperationException("can not rollback commited transaction.");

            if (IsRolledback)
                throw new InvalidOperationException("can not rollback twice.");

            IsRolledback = true;
        }
    }
}