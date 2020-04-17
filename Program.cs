using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OilTraderAI.Class;
using OilTraderAI.Misc;
using Microsoft.ML.Data;
using Microsoft.ML;
using static OilTraderAI.Class.ModelInput;
using static OilTraderAI.Class.ModelOutput;

namespace OilTraderAI
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("press any key to start the process");
            var userEntry = Console.ReadLine();

            //1-Create csv from slqLite db
            Helper.CreateCsv();

            //2 - Read csv and create model
            var modelPathList = Directory.GetFiles(Environment.CurrentDirectory + "/Csv/", "*", SearchOption.AllDirectories);
            CreateModel(modelPathList.Select(p => p).FirstOrDefault());

            //3 - FINI
            Console.WriteLine("CSV created and models created!");
            userEntry = Console.ReadLine();
        }

        private static void CreateModel(string sourcePath)
        {
            ITransformer model;
            MLContext mlContext = new MLContext();

            //1 - Load data from csv
            IDataView trainingData = mlContext.Data.LoadFromTextFile<ModelInput>(path: sourcePath, hasHeader: true, separatorChar: ',');

            //2 - Create pipeline
            var pipeline1 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));
            //3 - Evalaute the model during Debug phase
            //EvalauteNodel.Evaluate(mlContext, trainingData, pipeline1);
            //3 - Train your model based on the data set
            model = pipeline1.Fit(trainingData);
            //4: We save the model
            SaveModelAsFile(mlContext, model, trainingData, sourcePath, "Stochastic dual Coordinate");

//How to is here
//https://github.com/dotnet/machinelearning-samples/tree/master/samples/csharp/getting-started/Regression_TaxiFarePrediction


            var pipeline2 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.OnlineGradientDescent(labelColumnName: "Label", featureColumnName: "Features"));
            //EvalauteNodel.Evaluate(mlContext, trainingData, pipeline2);
            model = pipeline2.Fit(trainingData);
            SaveModelAsFile(mlContext, model, trainingData, sourcePath, "Gradient descent");

            // var pipeline3 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "Label", featureColumnName: "Features"));
            // //EvalauteNodel.Evaluate(mlContext, trainingData, pipeline3);
            // model = pipeline3.Fit(trainingData);
            // SaveModelAsFile(mlContext, model, trainingData, sourcePath, "Poisson regression");

            // // STEP 5: We load the model FOR DEBUGGING
            ITransformer loadedModel;
            loadedModel = LoadModelFromFile(mlContext, sourcePath, "Gradient descent");
            var predictionFunction = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(loadedModel);
            ModelOutput prediction = predictionFunction.Predict(new ModelInput
            {
                Rsi = (float)20,
                Macd = -0.008957828f,
                MacdSign = (float)-0.009085627,
                MacdHist = (float)0.000127799
            });

            Console.WriteLine(prediction.Future); 
        }
        
        #region helper

        private static EstimatorChain<ColumnConcatenatingTransformer> CreatePipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(ModelInput.Future)) //the output with LABEL as name
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(ModelInput.Rsi)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(ModelInput.Macd)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(ModelInput.MacdSign)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(ModelInput.MacdHist)))
            .Append(mlContext.Transforms.Concatenate("Features", nameof(ModelInput.Rsi), nameof(ModelInput.Macd), nameof(ModelInput.MacdSign), nameof(ModelInput.MacdHist)));
        }
        private static void SaveModelAsFile(MLContext mlContext, ITransformer model, IDataView trainingDataView, string sourcePath, string modelType)
        {
            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}/AIModel/{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, trainingDataView.Schema, fileStream);
        }

        private static ITransformer LoadModelFromFile(MLContext mlContext, string sourcePath, string modelType)
        {
            DataViewSchema modelSchema;

            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}\\AIModel\\{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return mlContext.Model.Load(stream, out modelSchema);
            }
        }

        #endregion
    }
}
