using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using Npgsql;
using System.Data;
using System.Diagnostics; 

namespace NeuralNetworkTutorialApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DataSet ds = new DataSet();
            DataSet ds2 = new DataSet();
            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            Stopwatch sWatch = new Stopwatch();
            TimeSpan tSpan;


            try
            {
                //  під'єднання до бд
                string connstring = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1postgres;Database=Labs;";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = "SELECT * FROM Task1 ORDER BY id";
                //string sql1 = "SELECT DISTINCT Type FROM train ORDER BY Type";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];

                /* da = new NpgsqlDataAdapter(sql1, conn);
                 da.Fill(ds2);
                 dt2 = ds2.Tables[0];*/
                conn.Close();
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
                throw;
            }

            foreach (DataColumn dt3 in dt.Columns)
            {
                Console.WriteLine(dt3.ColumnName);
            }

            int TRAINING_PATTERNS = dt.Columns.Count - 1;
            int PARAMETERS = dt.Rows.Count;
            //int NUM_OF_CLUSTERS = dt2.Rows.Count;
            //Console.WriteLine("Number of clusters = {0}", NUM_OF_CLUSTERS);
            double MIN_ERROR = 0.001;
            int TestAmount = 6;

            // Параметри для BackPropagation мережі
            int[] layerSizes = new int[3] { 2, 7, 1 }; // кількість шарів та нейронів у шарах

            // активаційні функції для кожного шару
            TransferFunction[] TFuncs = new TransferFunction[3] {TransferFunction.None,
                                                               TransferFunction.Sigmoid,
                                                               TransferFunction.Sigmoid};
            double LEARNING_RATE1 = 0.15; // швидкість навчання
            double MOMENTUM = 0.1; // крефіцієнт для навчання

            // Параметри для LVQ мережі
            double LEARNING_RATE2 = 1.0; // швидкість навчання
            double DECAY_RATE = 0.7; // швидкість зміни швидкості нвчання

            double[] h = new double[TRAINING_PATTERNS]; // критерій Ст'юдента


            double[][] inputs = new double[TRAINING_PATTERNS][];
            double[][] answers = new double[TRAINING_PATTERNS][];

            for (int i = 0; i < TRAINING_PATTERNS; i++)
            {
                inputs[i] = new double[PARAMETERS];
                answers[i] = new double[1];
            }



            // зчитування параметрів
            for (int i = 0; i < TRAINING_PATTERNS; i++)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                    inputs[i][k] = Convert.ToDouble(dt.Rows[k][i + 1]);

                //answers[i][0] = Convert.ToDouble(dt.Rows[i][dt.Columns.Count - 1]);
            }

            Console.WriteLine("Training Patterns:");
            for (int i = 0; i < TRAINING_PATTERNS; i++)
            {
                for (int p = 0; p < PARAMETERS; p++)
                    Console.Write(inputs[i][p] + " ");
                Console.WriteLine();
                Console.WriteLine();
            }

            /* for (int i = 0; i < inputs.GetUpperBound(0) + 1; i++)
             {
                 h[i] = Statistic.Interval(inputs[i]);
             }

             Console.WriteLine("Student parameters:");
             foreach(double param in h)
                 Console.WriteLine("param = {0}", param);

             // Масив частот
             Dictionary<double, int>[] pairs = new Dictionary<double, int>[TRAINING_PATTERNS];
             for (int i = 0; i < TRAINING_PATTERNS; i++)
             {
                 pairs[i] = new Dictionary<double, int>();
             }
             Console.WriteLine("Amount of pairs: {0}", pairs.Count());
             //ArrayList array = new ArrayList();
             for (int i = 0; i < TRAINING_PATTERNS; i++) {
                 for (double curr = inputs[i].Min(); curr <= inputs[i].Max(); curr += h[i])
                 {
                     int count = 0;
                     for (int j = 0; j < inputs[i].Length; j++)
                     {
                         if (curr <= inputs[i][j] && inputs[i][j] < curr + h[i])
                             count++;
                     }
                     pairs[i].Add((curr + h[i] + curr)/2.0, count);
                     Console.WriteLine("Key = {0}, Value = {1}", pairs[i].Last().Key, pairs[i].Last().Value);
                 }

                 Console.WriteLine();
             }*/

            double[][] array2 = Statistic.CreateArrayFromDictionary(inputs);
            for (int i = 0; i < array2.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < array2[i].Length; j++)
                    Console.Write(array2[i][j] + " ");
                Console.WriteLine();
            }



            /* Normalize.NormalizeParameters(inputs);

             BackPropagationNetwork bpn = new BackPropagationNetwork(layerSizes, TFuncs);

             double[] output = new double[1];

             sWatch.Start();
             bpn.TrainNetwork(inputs, Normalize.FormAnswersBackPropagation(answers), MIN_ERROR, LEARNING_RATE1, MOMENTUM);
             sWatch.Stop();

             tSpan = sWatch.Elapsed;
             Console.WriteLine("Time for BackPropagation " + tSpan.ToString()); // Виведення часу навчання

             sWatch.Reset(); // обнуляємо час

             bpn.Save(@"d:\Навчання\test_network.xml");

             BackPropagationNetwork bpn2 = new BackPropagationNetwork(@"d:\Навчання\test_network.xml");
             for (int k = 0; k < TRAINING_PATTERNS; k++)
             {
                 Console.WriteLine("cluster {0:0.000}", bpn2.getCluster(inputs[k], output));
             }

             double[][] testArray = GenerateTest.GenerateOutputICG(PARAMETERS, TestAmount);
             Normalize.NormalizeTest(testArray);

             for (int k = 0; k < TestAmount; k++)
             {
                 Console.WriteLine("---- cluster {0:}", bpn2.getCluster(testArray[k], output));
             }

             sWatch.Start();
              LVQ lvq = new LVQ(inputs, Normalize.FormAnswersLVQ(answers), 0.0000000001, LEARNING_RATE2, DECAY_RATE, NUM_OF_CLUSTERS);
             lvq.TrainNetwork();
             sWatch.Stop();

             tSpan = sWatch.Elapsed;

             Console.WriteLine("Time for LVQ " + tSpan.ToString());

              for(int i = 0; i < TRAINING_PATTERNS; i++)
              {
                  Console.WriteLine("The result for vector {0} : Cluster {1}", i, lvq.getCluster(inputs[i]));
              }

             for (int i = 0; i < TestAmount; i++)
             {
                 Console.WriteLine("---- The result for vector2 {0} : Cluster {1}", i, lvq.getCluster(testArray[i]));
             }

             lvq.Save(@"d:\Навчання\test_network2.xml");

              LVQ lvq2 = new LVQ(@"d:\Навчання\test_network2.xml");

             for (int i = 0; i < TestAmount; i++)
             {
                 Console.WriteLine("---- The result for vector2 {0} : Cluster {1}", i, lvq2.getCluster(testArray[i]));
             }
             */
            Console.ReadKey();

        }
    }
}
