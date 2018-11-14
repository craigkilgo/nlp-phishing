using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms.Text;

namespace model
{
      class Program
    {
        // <Snippet2>
        static readonly string _trainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "training-data.tsv");
        static readonly string _testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "testing-data.tsv");
        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");
        static TextLoader _textLoader;
        // </Snippet2>

        static void Main(string[] args)
        {
            // Create ML.NET context/local environment - allows you to add steps in order to keep everything together
            // during the learning process.
            //Create ML Context with seed for repeatable/deterministic results
            // <Snippet3>
            MLContext mlContext = new MLContext(seed: 0);
            // </Snippet3>

            // The TextLoader loads a dataset with comments and corresponding postive or negative sentiment.
            // When you create a loader, you specify the schema by passing a class to the loader containing
            // all the column names and their types. This is used to create the model, and train it.
            // Initialize our TextLoader
            // <Snippet4>
            _textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
                                                {
                                                    Separator = "tab",
                                                    HasHeader = true,
                                                    Column = new[]
                                                                {
                                                                  new TextLoader.Column("Label", DataKind.Bool, 0),
                                                                  new TextLoader.Column("UrlText", DataKind.Text, 1)
                                                                }
                                                }
            );
            // </Snippet4>

            // <Snippet5>
            var model = Train(mlContext, _trainDataPath);
            // </Snippet5>

            // <Snippet11>
            Evaluate(mlContext, model);
            // </Snippet11>

            // <Snippet16>
            Predict(mlContext, model);
            // </Snippet16>

            // <Snippet25>
            //PredictWithModelLoadedFromFile(mlContext);
            // </Snippet25>


            Console.WriteLine();
            Console.WriteLine("=============== End of process ===============");
        }

        public static ITransformer Train(MLContext mlContext, string dataPath)
        {
            //Note that this case, loading your training data from a file,
            //is the easiest way to get started, but ML.NET also allows you
            //to load data from databases or in-memory collections.
            // <Snippet6>
            IDataView dataView =_textLoader.Read(dataPath);
            // </Snippet6>

            // Create a flexible pipeline (composed by a chain of estimators) for creating/training the model.
            // This is used to format and clean the data.
            // Convert the text column to numeric vectors (Features column)
            // <Snippet7>
            var pipeline = mlContext.Transforms.Text.FeaturizeText("UrlText", "Features")
             //</Snippet7>

            // Adds a FastTreeBinaryClassificationTrainer, the decision tree learner for this project

                    .Append(mlContext.BinaryClassification.Trainers.FastTree(numLeaves: 50, numTrees: 50, minDatapointsInLeafs: 20));


            // Create and train the model based on the dataset that has been loaded, transformed.
            // <Snippet9>
            Console.WriteLine("=============== Create and Train the Model ===============");
            var model = pipeline.Fit(dataView);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();
            // </Snippet9>

            // Returns the model we trained to use for evaluation.
            // <Snippet10>
            return model;
            // </Snippet10>
        }

        public static void Evaluate(MLContext mlContext, ITransformer model)
        {
            // Evaluate the model and show accuracy stats
            // Load evaluation/test data
            // <Snippet12>
            IDataView dataView = _textLoader.Read(_testDataPath);
            // </Snippet12>

            //Take the data in, make transformations, output the data.
            // <Snippet13>
            Console.WriteLine("=============== Evaluating Model accuracy with Test data===============");
            var predictions = model.Transform(dataView);
            // </Snippet13>

            // BinaryClassificationContext.Evaluate returns a BinaryClassificationEvaluator.CalibratedResult
            // that contains the computed overall metrics.
            // <Snippet14>
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");
            // </Snippet14>

            // The Accuracy metric gets the accuracy of a classifier, which is the proportion
            // of correct predictions in the test set.

            // The Auc metric gets the area under the ROC curve.
            // The area under the ROC curve is equal to the probability that the classifier ranks
            // a randomly chosen positive instance higher than a randomly chosen negative one
            // (assuming 'positive' ranks higher than 'negative').

            // The F1Score metric gets the classifier's F1 score.
            // The F1 score is the harmonic mean of precision and recall:
            //  2 * precision * recall / (precision + recall).

            // <Snippet15>
            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of model evaluation ===============");
            //</Snippet15>

            // Save the new model to .ZIP file
            // <Snippet23>
            SaveModelAsFile(mlContext, model);
            // </Snippet23>
        }

        private static void Predict(MLContext mlContext, ITransformer model)
        {
            // <Snippet17>
            var predictionFunction = model.MakePredictionFunction<PhishData, PhishPrediction>(mlContext);
            // </Snippet17>

            // <Snippet18>
            PhishData sample = new PhishData
            {
                UrlText = "go0gl3.com"
            };
            PhishData sample2 = new PhishData
            {
                UrlText = "facebook.com/news/"
            };
            // </Snippet18>

            // <Snippet19>
            var resultprediction = predictionFunction.Predict(sample);
            var resultprediction2 = predictionFunction.Predict(sample2);
            // </Snippet19>
            // <Snippet20>
            Console.WriteLine();
            Console.WriteLine("=============== Prediction Test of model with a single sample and test dataset ===============");

            Console.WriteLine();
            Console.WriteLine($"Url: {sample.UrlText} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Phish" : "Not Phish")} | Probability: {resultprediction.Probability} ");
            Console.WriteLine($"Url: {sample2.UrlText} | Prediction: {(Convert.ToBoolean(resultprediction2.Prediction) ? "Phish" : "Not Phish")} | Probability: {resultprediction2.Probability} ");
            Console.WriteLine("=============== End of Predictions ===============");
            Console.WriteLine();
            // </Snippet20>
        }

        public static void PredictWithModelLoadedFromFile(MLContext mlContext)
        {
            // Adds some comments to test the trained model's predictions.
            // <Snippet26>
            IEnumerable<PhishData> phishes = new[]
            {
                new PhishData
                {
                    UrlText = "go0gl3.com"
                },
                new PhishData
                {
                    UrlText = "facebook.com/index.php"
                }
            };
            // </Snippet26>

            // <Snippet27>
            ITransformer loadedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }
            // </Snippet27>

            // <Snippet28>
            // Create prediction engine
            var phishStreamingDataView = mlContext.CreateStreamingDataView(phishes);
            var predictions = loadedModel.Transform(phishStreamingDataView);

            // Use the model to predict whether comment data is toxic (1) or nice (0).
            var predictedResults = predictions.AsEnumerable<PhishPrediction>(mlContext, reuseRowObject: false);
            // </Snippet28>

            // <Snippet29>
            Console.WriteLine();

            Console.WriteLine("=============== Prediction Test of loaded model with a multiple samples ===============");
            // </Snippet29>

            Console.WriteLine();

            // Builds pairs of (sentiment, prediction)
            // <Snippet30>
            var phishesAndPredictions = phishes.Zip(predictedResults, (phish, prediction) => (phish, prediction));
            // </Snippet30>

            // <Snippet31>
            foreach (var item in phishesAndPredictions)
            {
                //Console.WriteLine($"IsItPhish: {item.phish.PhishText} | Prediction: {(Convert.ToBoolean(item.prediction.Prediction) ? "Phish" : "Safe")} | Probability: {item.prediction.Probability} ");
            }
            Console.WriteLine("=============== End of predictions ===============");

            // </Snippet31>
        }

        // Saves the model we trained to a zip file.

        private static void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            // <Snippet24>
            using (var fs = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model,fs);
            // </Snippet24>

            Console.WriteLine("The model is saved to {0}", _modelPath);
        }

    }
}
