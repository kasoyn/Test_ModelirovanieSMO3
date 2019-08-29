using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_ModelirovanieSMO
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        ///
        //добавить дисперсию и среднее отклонение
        private void button1_Click(object sender, EventArgs e)
        {
            double p = double.Parse(textBox1.Text);     //нагрузка на систему
            double m = double.Parse(textBox2.Text);     //длина очереди
            double N = 0, Po = 0, sum = 0, Pk;  //теоретическое значение для среднего значения заявок в систему
                                                //Ро (Рк) - вероятность того, что в смомент времени т в системе будет ноль(к) заявок, промежуточная переменная для подсчета суммы 

            double[] teory = new double[3000];    //объявление списка
            

            chart1.Series[0].Points.Clear();            chart1.Series[1].Points.Clear();            chart1.Series[2].Points.Clear();            chart1.Series[3].Points.Clear();

            chart1.ChartAreas[0].AxisX.Minimum = -1;//настройка осей графика
            chart1.ChartAreas[0].AxisX.Maximum = 12;
            chart1.ChartAreas[0].AxisY.Minimum = 0;//настройка осей графика
            chart1.ChartAreas[0].AxisY.Maximum = m+2;
            chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 1;//определяем шаг сетки


            //теория
            int u = 0;
            for (double z = 0.01; z <= p; z += 0.1)
            {
                
                sum = 0; N = 0;
                for (double k = 0; k <= (m + 1); k++)
                {
                    sum += Math.Pow(z, k);
                }
                
                Po = 1 / sum;
                for (double k = 0; k <= (m + 1); k++)
                {
                    Pk = Math.Pow(z, k) * Po;
                    N += k * Pk;
                }
                //textBox3.Text += "N=" + N + Environment.NewLine;
                teory[u] = N;
                //textBox3.Text += "i=" + u + "teory=" + teory[u] + Environment.NewLine;
                u++;
                chart1.Series[0].Points.AddXY(z, N);
                chart1.Series[2].Points.AddXY(z, m + 1);
            }
            //создать список(словарь) для теоретических точек, чтобы посчитать дисперсию
            
            //эксперимент

            var miy = 1.0;  //интенсивность потока обслуживания, как сильно нагружена система обслуживания, сколько заявок обслуживалось за опред. период времени.
            var lambda = 1.0;   //интенсивность потока обслуживания, как сильно нагружена система обслуживания, сколько человек обслуживалось за опред. период времени.
            int ten = 10;  //это количество отчетов, чтобы получить одну точку на графике, число заявок, которые должны быть обслужены для получения 1 т. на граф. 

            //количество отчетов
            for (double x = 0.01; x <= p; x += 0.1)
            {
                lambda = x * miy;   //х = ро, а ро=лямбда/мю
                var n = 1000;
                var Nexp = 0.0;     //экспериментальное значение для среднего значения заявок в систему
                var sumoch = 0.0;   //вспомагательная переменная, сумма всех заявок, которые когда-либо были в очереди 
                var tau_z = 0.0;    //случайное время для потока заявок 

                var tau_obs = 0.0;    //случайное время для потока обслуживания (сколько времени ушло на обслуживание какой-либо заявки) 
                var vobs = false;   //true = на обслуживании находится заявка, false = заявок на обслуживании нет
                var ochered = 0.0;  //количество заявок в очереди в определенный период времени
                double disp = 0.0;     //дисперсия
                Random rnd = new Random();

                for (int i = 0; i < n; i++)   //перебирает количество отчетов, т.е. при i = 0 ни в системе, ни в очереди заявок нет; при i = 1 одна заявка в очереди и т.д.
                {
                    var k_izm = 0;  //выяснить что за переменная
                    while (k_izm != ten)    //пока 
                    {
                        if (tau_z == 0)     //если в период случайного времени заявок в системе нет(т.е. на обсл и в очер)
                        {
                            if ((ochered == 0) && (!vobs))      //если в очереди нет заявок, но есть заявка на обслучивании
                            {
                                tau_z = -1 / lambda * Math.Log(rnd.NextDouble());    //время прихода данной заявки(времени проведенного в очереди) = рандомному числу от 0 до 1
                                tau_obs = -1 / miy * Math.Log(rnd.NextDouble());    //время на обслуживание данной заявки(времени проведенного в обслуживании) = рандомному числу от 0 до 1
                                vobs = true;
                            }
                            else if (ochered < m + 1)   //если же в очереди есть заявка, то добавляем ее к количеству заявок в очереди и записываем время в очереди 
                            {
                                ochered++;
                                tau_z = -1 / lambda * Math.Log(rnd.NextDouble());
                            }
                            else    //если очередь занята, то заявка некоторое время висит в ожидании очереди и выбрасывается, мы ее никуда не записываем
                            {
                                tau_z = -1 / lambda * Math.Log(rnd.NextDouble());
                            }

                        }

                        if (tau_obs == 0)   //если заявка закончила обслуживаться, то время обслуживания = 0 
                        {
                            if (ochered > 0)    // и если очередь больше 0
                            {
                                ochered--;  //уменьшаем очередь на 1,т.к. заявка из очереди прешла на ослуживание
                                tau_obs = -1 / miy * Math.Log(rnd.NextDouble());    //и присваеваем рандомное время обслуживания 
                                vobs = true;
                            }
                            else vobs = false;
                        }

                        if (vobs)      //если tau_obs=1 и tau_z=1, может ли такое быть и что тогда??? 
                        {
                            if (tau_z > tau_obs)
                            {
                                tau_z -= tau_obs;   //определение времени ожидания 
                                tau_obs = 0;
                            }
                            else if (tau_z < tau_obs)
                            {
                                tau_obs -= tau_z;
                                tau_z = 0;
                            }

                        }
                        else tau_z = 0;

                        if (tau_obs == 0 && vobs)   //если в обслуживании нет заявок, и либо ни одна заявка еще не обслуживалась, либо все заявки уже завершили обслуживание
                        {
                            k_izm++;    //прибавляем одну заявку к количеству обслуженных заявок
                            if (k_izm == ten)       //если мы обслужили тен заявок при определенно ро
                            {
                                sumoch += ochered;      //сумма всех заявок которые были в очереди при определенном ро                              
                                break;
                            }
                            vobs = false;
                        }

                    }

                }


                //textBox3.Text += "Nexp=" + Nexp + Environment.NewLine;
                //sum = 0;

                //дисперсия
                //считаем теорию для текущей точки(для текущей ро)

                //вычитаем из практической точки (очередь-теор.точка)
                //возводим это все в кв. и прибавляем к дисперсии
                //disp = ;
                Nexp = sumoch / n; //сумму всех заявок в очереди делим на количество отчетов
                //teory[i] - массив теоретических значений
                for (int i = 0; i < ten; i++)
                {
                    disp += (Nexp-teory[i]) * (Nexp - teory[i]);
                }

                var disp2 = 0.0;
                disp2 = Math.Sqrt((disp /n));
                //chart1.Series[3].Points.AddXY(x, disp);
                disp = 0.0;
                chart1.Series[3].Points.AddXY(x, disp2);

                //дисперсия на н-1
                /* textBox3.Text += "Disp=" + disp + Environment.NewLine;
                textBox3.Text += "Disp/(n-1)=" + disp/(ten-1) + Environment.NewLine;
                textBox3.Text += "S=" + Math.Sqrt(disp) + Environment.NewLine;
                */
                chart1.Series[1].Points.AddXY(x,Nexp);
                
            }


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
