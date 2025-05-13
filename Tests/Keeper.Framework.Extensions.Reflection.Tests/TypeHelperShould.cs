namespace Keeper.Framework.Extensions.Reflection.Tests
{
    public class TypeHelperShould
    {
        [Fact]
        public void ModelMethodCall1Correctly()
        {
            var date = DateTime.Parse("2000-01-01");
            
            var modelInfo = TypeHelper.GetMethodCallInfo((TypeHelperShould x) => x.FakeMethodCall1("abc", date));

            Assert.NotNull(modelInfo);
            Assert.Equal(typeof(TypeHelperShould).FullName, modelInfo.ClassName);
            Assert.Equal(typeof(TypeHelperShould).AssemblyQualifiedName, modelInfo.AssemblyQualifiedName);
            Assert.Equal(nameof(FakeMethodCall1), modelInfo.MethodName);
            Assert.Equal(typeof(int), modelInfo.ReturnValue);
            Assert.Collection(modelInfo.Arguments,
                x => Assert.Equal("abc", x),
                x => Assert.Equal(date, x)
            );
        }

        [Fact]
        public void ModelMethodCall2Correctly()
        {
            var date = DateTime.Parse("2000-01-01");

            var modelInfo = TypeHelper.GetMethodCallInfo((TypeHelperShould x) => x.FakeMethodCall2("abc", date));

            Assert.NotNull(modelInfo);
            Assert.Equal(typeof(TypeHelperShould).FullName, modelInfo.ClassName);
            Assert.Equal(typeof(TypeHelperShould).AssemblyQualifiedName, modelInfo.AssemblyQualifiedName);
            Assert.Equal(nameof(FakeMethodCall2), modelInfo.MethodName);
            Assert.Equal(typeof(Task<int>), modelInfo.ReturnValue);
            Assert.Collection(modelInfo.Arguments,
                x => Assert.Equal("abc", x),
                x => Assert.Equal(date, x)
            );
        }

        [Fact]
        public void ModelMethodCall2WithAnonymousObjectCorrectly()
        {
            var obj = new
            {
                arg1 = "abc",
                arg2 = DateTime.Parse("2000-01-01"),
            };

            var modelInfo = TypeHelper.GetMethodCallInfo((TypeHelperShould x) => x.FakeMethodCall2(obj.arg1, obj.arg2));

            Assert.NotNull(modelInfo);
            Assert.Equal(typeof(TypeHelperShould).FullName, modelInfo.ClassName);
            Assert.Equal(typeof(TypeHelperShould).AssemblyQualifiedName, modelInfo.AssemblyQualifiedName);
            Assert.Equal(nameof(FakeMethodCall2), modelInfo.MethodName);
            Assert.Equal(typeof(Task<int>), modelInfo.ReturnValue);
            Assert.Collection(modelInfo.Arguments,
                x => Assert.Equal(obj.arg1, x),
                x => Assert.Equal(obj.arg2, x)
            );
        }

        [Fact]
        public void ModelMethodCall2ThroughMethodCallCorrectly()
        {
            var date = DateTime.Parse("2000-01-01");

            var modelInfo = GetMethodCallModel("abc", date);

            Assert.NotNull(modelInfo);
            Assert.Equal(typeof(TypeHelperShould).FullName, modelInfo.ClassName);
            Assert.Equal(typeof(TypeHelperShould).AssemblyQualifiedName, modelInfo.AssemblyQualifiedName);
            Assert.Equal(nameof(FakeMethodCall2), modelInfo.MethodName);
            Assert.Equal(typeof(Task<int>), modelInfo.ReturnValue);
            Assert.Collection(modelInfo.Arguments,
                x => Assert.Equal("abc", x),
                x => Assert.Equal(date, x)
            );
        }

        [Fact]
        public void ModelMethodCall2WithModelCorrectly()
        {
            var obj = new TestArgsClass
            {
                Arg1 = "abc",
                Arg2 = DateTime.Parse("2000-01-01"),
            };

            var modelInfo = TypeHelper.GetMethodCallInfo((TypeHelperShould x) => x.FakeMethodCall2(obj.Arg1, obj.Arg2));

            Assert.NotNull(modelInfo);
            Assert.Equal(typeof(TypeHelperShould).FullName, modelInfo.ClassName);
            Assert.Equal(typeof(TypeHelperShould).AssemblyQualifiedName, modelInfo.AssemblyQualifiedName);
            Assert.Equal(nameof(FakeMethodCall2), modelInfo.MethodName);
            Assert.Equal(typeof(Task<int>), modelInfo.ReturnValue);
            Assert.Collection(modelInfo.Arguments,
                x => Assert.Equal(obj.Arg1, x),
                x => Assert.Equal(obj.Arg2, x)
            );
        }

        class TestArgsClass
        {
            public required string Arg1 { get; init; }

            public DateTime Arg2 { get; init; }
        }

        private MethodCallModel GetMethodCallModel(string arg1, DateTime arg2)
        {
            return TypeHelper.GetMethodCallInfo((TypeHelperShould x) => x.FakeMethodCall2(arg1, arg2));
        }

        public int FakeMethodCall1(string arg1, DateTime arg2)
        {
            return 1;
        }

        public Task<int> FakeMethodCall2(string arg1, DateTime arg2)
        {
            return Task.FromResult(1);
        }
    }
}