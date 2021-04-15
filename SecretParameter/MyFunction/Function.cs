using System;
using System.Threading.Tasks;
using LambdaSharp;

namespace ServerlessPatterns.DeploymentConfiguration.SecretParameter.MyFunction {

    public class FunctionRequest { }

    public class FunctionResponse { }

    public sealed class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {

        //--- Fields ---
        private string _topicDisplayName;

        //--- Constructors ---
        public Function() : base(new LambdaSharp.Serialization.LambdaSystemTextJsonSerializer()) { }

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) {
            _topicDisplayName = config.ReadText("TopicDisplayName");
        }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request) {
            LogInfo($"TopicDisplayName: {_topicDisplayName}");
            return new FunctionResponse();
        }
    }
}
