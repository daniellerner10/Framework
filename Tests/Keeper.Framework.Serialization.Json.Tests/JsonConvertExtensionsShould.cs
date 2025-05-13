using Keeper.Framework.Extensions.Collections;
using Newtonsoft.Json;

namespace Keeper.Framework.Serialization.Json.Tests
{
    public class JsonConvertExtensionsShould
    {
        [Fact]
        public void DesializeStreamAsIEnumerable()
        {
            var expected = new List<TestClass>()
                {
                    new() { Val1 = 1, Val2 = "string1" },
                    new() { Val1 = 2, Val2 = "string2" },
                    new() { Val1 = 3, Val2 = "string3" },
                };

            var json = JsonConvert.SerializeObject(expected);

            using var memStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memStream, leaveOpen: true))
                streamWriter.Write(json);

            memStream.Seek(0, SeekOrigin.Begin);

            var result = memStream.ReadJsonArrayFromStream<TestClass>().ToList();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DesializeStreamAsIEnumerableWithComment()
        {
            var json = """
                [
                    /* some comment */
            
                    {"Val1":1,"Val2":"string1"},
                    {"Val1":2,"Val2":"string2"},
                    {"Val1":3,"Val2":"string3"}

                ]
            """;

            using var memStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memStream, leaveOpen: true))
                streamWriter.Write(json);

            memStream.Seek(0, SeekOrigin.Begin);

            var result = memStream.ReadJsonArrayFromStream<TestClass>().ToList();

            Assert.Collection(result,
                x => { Assert.Equal(1, x.Val1); Assert.Equal("string1", x.Val2); },
                x => { Assert.Equal(2, x.Val1); Assert.Equal("string2", x.Val2); },
                x => { Assert.Equal(3, x.Val1); Assert.Equal("string3", x.Val2); }
            );
        }

        [Fact]
        public async Task DesializeStreamAsIAsyncEnumerable()
        {
            var expected = new List<TestClass>()
                {
                    new() { Val1 = 1, Val2 = "string1" },
                    new() { Val1 = 2, Val2 = "string2" },
                    new() { Val1 = 3, Val2 = "string3" },
                };

            var json = JsonConvert.SerializeObject(expected);

            using var memStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memStream, leaveOpen: true))
                streamWriter.Write(json);

            memStream.Seek(0, SeekOrigin.Begin);

            var result = await memStream.ReadJsonArrayFromStreamAsync<TestClass>().ToListAsync();

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task DesializeStreamAsIAsyncEnumerableWithComment()
        {
            var json = """
                [
                    /* some comment */

                    {"Val1":1,"Val2":"string1"},
                    {"Val1":2,"Val2":"string2"},
                    {"Val1":3,"Val2":"string3"}

                ]
            """;

            using var memStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memStream, leaveOpen: true))
                streamWriter.Write(json);

            memStream.Seek(0, SeekOrigin.Begin);

            var result = await memStream.ReadJsonArrayFromStreamAsync<TestClass>().ToListAsync();

            Assert.Collection(result,
                x => { Assert.Equal(1, x.Val1); Assert.Equal("string1", x.Val2); },
                x => { Assert.Equal(2, x.Val1); Assert.Equal("string2", x.Val2); },
                x => { Assert.Equal(3, x.Val1); Assert.Equal("string3", x.Val2); }
            );
        }

        record TestClass
        {
            public int Val1 { get; set; }
            public string? Val2 { get; set; }
        }
    }
}