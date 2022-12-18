using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqsStepFunctionsPipes
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new SqsStepFunctionsPipesStack(app, "SqsStepFunctionsPipesStack", new StackProps{});
            app.Synth();
        }
    }
}
