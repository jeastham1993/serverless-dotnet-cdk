using Amazon.CDK;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace SqsStepFunctionsPipes
{
    public class SqsStepFunctionsPipesStack : Stack
    {
        internal SqsStepFunctionsPipesStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var queue = new Queue(this, "SourceSqsQueue");

            var mapState = new Map(this, "BaseMapState", new MapProps()
            {
                ItemsPath = JsonPath.EntirePayload
            });
            mapState.Iterator(new Wait(this, "5SecondWait", new WaitProps()
            {
                Time = WaitTime.Duration(Duration.Seconds(5)),
            }));

            var targetStepFunction = new StateMachine(this, "TargetStateMachine", new StateMachineProps()
            {
                StateMachineName = "PipesTargetStateMachine",
                Definition = mapState
            });
            
            var pipe = new PipeBuilder(this)
                .AddSqsSource(queue)
                .AddStepFunctionTarget(targetStepFunction)
                .Build();
        }
    }
}
