using System.Collections.Generic;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Pipes;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace SqsStepFunctionsPipes;

public class PipeBuilder
{
    private PolicyDocument _sourcePolicy;
    private PolicyDocument _targetPolicy;
    private CfnPipe.PipeSourceParametersProperty _sourceParametersProperty;
    private CfnPipe.PipeTargetParametersProperty _targetParametersProperty;
    private string _source;
    private string _target;
    private Construct _scope;

    public PipeBuilder(Construct scope)
    {
        this._scope = scope;
    }

    public PipeBuilder AddSqsSource(Queue queue)
    {
        _source = queue.QueueArn;
        
        _sourcePolicy = new PolicyDocument(
            new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(
                        new PolicyStatementProps
                        {
                            Resources = new[] { queue.QueueArn },
                            Actions = new[] { "sqs:ReceiveMessage", "sqs:DeleteMessage", "sqs:GetQueueAttributes" },
                            Effect = Effect.ALLOW
                        })
                }
            });

        _sourceParametersProperty = new CfnPipe.PipeSourceParametersProperty()
        {
            SqsQueueParameters = new CfnPipe.PipeSourceSqsQueueParametersProperty()
            {
                BatchSize = 5,
                MaximumBatchingWindowInSeconds = 10
            }
        };

        return this;
    }

    public PipeBuilder AddStepFunctionTarget(StateMachine stepFunction)
    {
        _target = stepFunction.StateMachineArn;
        
        _targetPolicy = new PolicyDocument(
            new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(
                        new PolicyStatementProps
                        {
                            Resources = new[] { stepFunction.StateMachineArn },
                            Actions = new[] { "states:StartExecution" },
                            Effect = Effect.ALLOW
                        })
                }
            });

        _targetParametersProperty = new CfnPipe.PipeTargetParametersProperty()
        {
            StepFunctionStateMachineParameters = new CfnPipe.PipeTargetStateMachineParametersProperty()
            {
                InvocationType = "FIRE_AND_FORGET"
            }
        };

        return this;
    }

    public CfnPipe Build()
    {
        var pipeRole = new Role(
            _scope,
            "PipeRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("pipes.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument>(2)
                {
                    { "SourcePolicy", _sourcePolicy },
                    { "TargetPolicy", _targetPolicy }
                }
            });
        
        return new CfnPipe(_scope, "MyNewPipe", new CfnPipeProps()
        {
            RoleArn = pipeRole.RoleArn,
            Source = _source,
            SourceParameters = _sourceParametersProperty,
            Target = _target,
            TargetParameters = _targetParametersProperty,
        });
    }
}