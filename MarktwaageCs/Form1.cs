using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarktwaageCs
{
    public partial class Form1 : Form
    {
        private string csv;
        private CancellationTokenSource cts;
        private bool dialogUeberspringen;
        public Form1()
        {
            InitializeComponent();
        }
        [DebuggerStepThrough]
        private bool GewichtVerwendet(Dictionary<long, sbyte> gewogen, long[] gewichte, long index, sbyte gewicht)
        {
            while(index > 0)
            {
                if(Math.Abs(gewogen[index]) == gewicht)
                {
                    return true;
                }
                if(gewogen[index] > 0)
                {
                    index -= gewichte[gewogen[index]];
                }
                else
                {
                    index += gewichte[-gewogen[index]];
                }
            }
            return false;
        }

        private string AusgabeText(Dictionary<long, sbyte> gewogen, long[] gewichte, long indexGewogen)
        {
            IEnumerable<sbyte> verwendeteGewichte = VerwendeteGewichte(gewogen, gewichte, indexGewogen);
            IEnumerable<long> gewichteLinks = verwendeteGewichte.Where(x => x < 0).Select(x => gewichte[-x]);
            IEnumerable<long> gewichteRechts = verwendeteGewichte.Where(x => x > 0).Select(x => gewichte[x]);
            Dictionary<long, sbyte> verwendeteGewichteLinks = new Dictionary<long, sbyte>();
            Dictionary<long, sbyte> verwendeteGewichteRechts = new Dictionary<long, sbyte>();

            foreach (long gewicht in gewichteLinks)
            {
                if (!verwendeteGewichteLinks.ContainsKey(gewicht))
                {
                    verwendeteGewichteLinks.Add(gewicht, 0);
                }
                verwendeteGewichteLinks[gewicht]++;
            }
            foreach (long gewicht in gewichteRechts)
            {
                if (!verwendeteGewichteRechts.ContainsKey(gewicht))
                {
                    verwendeteGewichteRechts.Add(gewicht, 0);
                }
                verwendeteGewichteRechts[gewicht]++;
            }
            List<string> ausgabeStringsLinks = new List<string>();
            List<string> ausgabeStringsRechts = new List<string>();
            foreach (KeyValuePair<long, sbyte> gewicht in verwendeteGewichteLinks)
            {
                ausgabeStringsLinks.Add(gewicht.Value + " mal " + gewicht.Key + "g");
            }
            foreach (KeyValuePair<long, sbyte> gewicht in verwendeteGewichteRechts)
            {
                ausgabeStringsRechts.Add(gewicht.Value + " mal " + gewicht.Key + "g");
            }
            string ausgabeLinks = "";
            string ausgabeRechts = "";
            for (int index = 0; index < ausgabeStringsLinks.Count; index++)
            {
                if (index == 0)
                {
                    ausgabeLinks += ausgabeStringsLinks[index];
                }
                else if (index == ausgabeStringsLinks.Count - 1)
                {
                    ausgabeLinks += " und " + ausgabeStringsLinks[index];
                }
                else
                {
                    ausgabeLinks += ", " + ausgabeStringsLinks[index];
                }
            }
            for (int index = 0; index < ausgabeStringsRechts.Count; index++)
            {
                if (index == 0)
                {
                    ausgabeRechts += ausgabeStringsRechts[index];
                }
                else if (index == ausgabeStringsRechts.Count - 1)
                {
                    ausgabeRechts += " und " + ausgabeStringsRechts[index];
                }
                else
                {
                    ausgabeRechts += ", " + ausgabeStringsRechts[index];
                }
            }
            if (ausgabeLinks == "") ausgabeLinks = "keine Gewichte";
            if (ausgabeRechts == "") ausgabeRechts = "keine Gewichte";
            return ausgabeRechts + " auf der rechten Seite und " + ausgabeLinks + " auf der linken Seite";
        }

        private IEnumerable<sbyte> VerwendeteGewichte(Dictionary<long, sbyte> gewogen, long[] gewichte, long index)
        {
            while (index > 0)
            {
                yield return gewogen[index];
                if(gewogen[index] > 0)
                {
                    index -= gewichte[gewogen[index]];
                }
                else
                {
                    index += gewichte[-gewogen[index]];
                }
            }
        }

        private string CsvExportieren(Dictionary<long, sbyte> gewogen, long[] gewichte)
        {
            StringBuilder ausgeben = new StringBuilder();
            ausgeben.Append(";");
            for(sbyte index = 1; index < gewichte.Length; index++)
            {
                ausgeben.Append(gewichte[index]);
                ausgeben.Append(";");
            }
            ausgeben.Append("Differenz");
            ausgeben.AppendLine();

            for(long index = 10; index<=10000; index+=10)
            {
                if(gewogen.ContainsKey(index))
                {
                    ausgeben.Append(index);
                    ausgeben.Append(";");
                    IEnumerable<sbyte> verwendeteGewichte = VerwendeteGewichte(gewogen, gewichte, index);
                    for(sbyte indexGewichte = 1; indexGewichte < gewichte.Length;indexGewichte++)
                    {
                        if (verwendeteGewichte.Contains(indexGewichte))
                        {
                            ausgeben.Append("R");
                        }
                        else if (verwendeteGewichte.Contains((sbyte)-indexGewichte))
                        {
                            ausgeben.Append("L");
                        }
                        ausgeben.Append(";");
                    }
                    ausgeben.Append("0");
                    ausgeben.AppendLine();
                }
                else
                {
                    long differenz = 1;
                    ausgeben.Append(index);
                    ausgeben.Append(";");
                    while(true)
                    {
                        if(gewogen.ContainsKey(index + differenz))
                        {
                            IEnumerable<sbyte> verwendeteGewichte = VerwendeteGewichte(gewogen, gewichte, index + differenz);
                            for(sbyte indexGewichte = 1; indexGewichte < gewichte.Length; indexGewichte++)
                            {
                                if(verwendeteGewichte.Contains(indexGewichte))
                                {
                                    ausgeben.Append("R");
                                }
                                else if(verwendeteGewichte.Contains((sbyte)-indexGewichte))
                                {
                                    ausgeben.Append("L");
                                }
                                ausgeben.Append(";");
                            }
                            ausgeben.Append(differenz);
                        }
                        else if(index - differenz == 0)
                        {
                            ausgeben.Append(string.Concat(Enumerable.Repeat(";", gewichte.Length - 1)));
                            ausgeben.Append("-");
                            ausgeben.Append(differenz);
                        }
                        else if(gewogen.ContainsKey(index - differenz))
                        {
                            IEnumerable<sbyte> verwendeteGewichte = VerwendeteGewichte(gewogen, gewichte, index - differenz);
                            for(sbyte indexGewichte = 1; indexGewichte < gewichte.Length; indexGewichte++)
                            {
                                if(verwendeteGewichte.Contains(indexGewichte))
                                {
                                    ausgeben.Append("R");
                                }
                                else if(verwendeteGewichte.Contains((sbyte)-indexGewichte))
                                {
                                    ausgeben.Append("L");
                                }
                                ausgeben.Append(";");
                            }
                            ausgeben.Append("-");
                            ausgeben.Append(differenz);
                        }
                        else
                        {
                            differenz++;
                            continue;
                        }
                        break;
                    }
                }
            }
            return ausgeben.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            bool abgebrochen = false;
            try
            {
                Button1.Enabled = false;
                Button2.Enabled = false;
                Button3.Enabled = true;
                cts = new CancellationTokenSource();
                CancellationToken ct = cts.Token;
                if(dialogUeberspringen || OpenFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Text = "BwInf: Marktwaage - Verarbeitung";
                    ListBox1.Items.Clear();
                    Label2.Text = "Datei: " + OpenFileDialog1.SafeFileName;
                    Label1.Text = "Zeit: /";
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    StreamReader einlesen = new StreamReader(OpenFileDialog1.FileName);
                    sbyte gewichteAnzahl = 0;
                    einlesen.ReadLine();
                    while(!einlesen.EndOfStream)
                    {
                        gewichteAnzahl += sbyte.Parse(einlesen.ReadLine().Split(' ')[1]);
                    }
                    einlesen.BaseStream.Seek(0, SeekOrigin.Begin);
                    einlesen.ReadLine();
                    long[] gewichte = new long[gewichteAnzahl + 1];
                    sbyte gewichtNr = 1;
                    string zeile;
                    while(!einlesen.EndOfStream)
                    {
                        zeile = einlesen.ReadLine();
                        for (long i = 1; i <= long.Parse(zeile.Split(' ')[1]); i++)
                        {
                            gewichte[gewichtNr] = long.Parse(zeile.Split(' ')[0]);
                            gewichtNr += 1;
                        }
                    }
                    einlesen.Close();
                    gewichte = gewichte.OrderBy(x => x, new GewichtComparer()).ToArray();
                    Dictionary<long, sbyte> gewogen = new Dictionary<long, sbyte>()
                    {
                        { 0, 0 }
                    };
                    IList<long> keys;
                    ProgressBar1.Maximum = gewichte.Length * 2;
                    for(sbyte index = 0; index < gewichte.Length; index++)
                    {
                        keys = gewogen.Keys.ToList(); //.ToList => Kopie; sonst => Exception in foreach
                        ProgressBar2.Value = 0;
                        ProgressBar2.Maximum = keys.Count;
                        foreach (long indexGewogen in keys)
                        {
                            if(!GewichtVerwendet(gewogen, gewichte, indexGewogen, index))
                            {
                                long zielGewicht = indexGewogen + gewichte[index];
                                if(zielGewicht > 0 && !gewogen.ContainsKey(zielGewicht))
                                {
                                    gewogen[zielGewicht] = index;
                                }
                            }
                            Application.DoEvents();
                            if(ct.IsCancellationRequested)
                            {
                                abgebrochen = true;
                                return;
                            }
                            ProgressBar2.PerformStep();
                        }
                        ProgressBar1.PerformStep();
                        Label3.Text = (ProgressBar1.Maximum - ProgressBar1.Value).ToString();
                    }

                    for (sbyte index = 0; index < gewichte.Length; index++)
                    {
                        keys = gewogen.Keys.ToList(); //.ToList => Kopie; sonst => Exception in foreach
                        ProgressBar2.Value = 0;
                        ProgressBar2.Maximum = keys.Count;
                        foreach (long indexGewogen in keys)
                        {
                            if (!GewichtVerwendet(gewogen, gewichte, indexGewogen, index))
                            {
                                long zielGewicht = indexGewogen - gewichte[index];
                                if (zielGewicht > 0 && !gewogen.ContainsKey(zielGewicht))
                                {
                                    gewogen[zielGewicht] = (sbyte)-index;
                                }
                            }
                            Application.DoEvents();
                            if (ct.IsCancellationRequested)
                            {
                                abgebrochen = true;
                                return;
                            }
                            ProgressBar2.PerformStep();
                        }
                        ProgressBar1.PerformStep();
                        Label3.Text = (ProgressBar1.Maximum - ProgressBar1.Value).ToString();
                    }
                    ListBox1.BeginUpdate();
                    for(long index = 10; index <= 10000; index += 10)
                    {
                        if(gewogen.ContainsKey(index))
                        {
                            if (VerwendeteGewichte(gewogen, gewichte, index).Select(x => x > 0 ? gewichte[x] : -gewichte[-x]).Aggregate(0L, (alt, neu) => alt + neu) != index) throw new UngueltigesErgebnisException();
                            ListBox1.Items.Add(index + ": " + AusgabeText(gewogen, gewichte, index));
                        }
                        else
                        {
                            long differenz = 1;
                            while (true)
                            {
                                if (gewogen.ContainsKey(index + differenz))
                                {
                                    if (VerwendeteGewichte(gewogen, gewichte, index + differenz).Select(x => x > 0 ? gewichte[x] : -gewichte[-x]).Aggregate(0L, (alt, neu) => alt + neu) != index + differenz) throw new UngueltigesErgebnisException();
                                    ListBox1.Items.Add(index + ": " + AusgabeText(gewogen, gewichte, index + differenz) + " (Differenz: " + differenz + " nach oben)");
                                }
                                else if (index - differenz == 0)
                                {
                                    ListBox1.Items.Add(index + ": " + "keine Gewichte (Differenz: " + differenz + " nach unten)");
                                }
                                else if (gewogen.ContainsKey(index - differenz))
                                {
                                    if (VerwendeteGewichte(gewogen, gewichte, index - differenz).Select(x => x > 0 ? gewichte[x] : -gewichte[-x]).Aggregate(0L, (alt, neu) => alt + neu) != index - differenz) throw new UngueltigesErgebnisException();
                                    ListBox1.Items.Add(index + ": " + AusgabeText(gewogen, gewichte, index - differenz) + " (Differenz: " + differenz + " nach unten)");
                                }
                                else
                                {
                                    differenz += 1;
                                    continue;
                                }
                                break;
                            }
                        }
                    }
                    ListBox1.EndUpdate();
                    timer.Stop();
                    Label1.Text = "Zeit: " + (timer.ElapsedMilliseconds / 60000L) + " Minuten " + (timer.ElapsedMilliseconds % 60000 / 1000) + " Sekunden " + (timer.ElapsedMilliseconds % 60000 % 1000) + " Millisekunden";
                    csv = CsvExportieren(gewogen, gewichte);
                }
            }
            finally
            {
                Button1.Enabled = true;
                Button2.Enabled = !abgebrochen;
                Button3.Enabled = false;
                if(abgebrochen)
                {
                    Label2.Text = "Datei: /";
                    Text = "BwInf: Marktwaage";
                }
                else
                {
                    Text = "BwInf: Marktwaage - " + OpenFileDialog1.SafeFileName;
                }
                Label3.Text = "";
                ProgressBar2.Value = 0;
                ProgressBar1.Value = 0;
                dialogUeberspringen = false;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if(SaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if(File.Exists(SaveFileDialog1.FileName))
                {
                    File.Delete(SaveFileDialog1.FileName);
                }
                File.WriteAllText(SaveFileDialog1.FileName, csv);
            }
        }

        private void Abbrechen(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if(Environment.GetCommandLineArgs().Length > 1)
            {
                OpenFileDialog1.FileName = Environment.GetCommandLineArgs()[1];
                dialogUeberspringen = true;
                Button1_Click(null, null);
            }
        }
    }
}
