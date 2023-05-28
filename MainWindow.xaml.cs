using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EuroGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=eurovizio;";
		List<Euroviz> dataList = new List<Euroviz>();
		MySqlConnection connection;
		public MainWindow()
		{
			InitializeComponent();

			AdatbazisMegnyitas();
			TermekekBetolteseListaba();
			this.Closed += (sender, args) => { AdatbazisLezarasa(); };
		}
		private void AdatbazisMegnyitas()
		{
			try
			{
				connection = new MySqlConnection(connectionString);
				connection.Open();
			}
			catch (Exception ex)
			{

				Console.WriteLine("Nem tudott csatlakozni az adatbázishoz!");
				Console.WriteLine(ex.Message);
				this.Close();
			}
		}
		private void TermekekBetolteseListaba()
		{
			string queryText = "SELECT dal.ev,eloado,cim,helyezes,pontszam FROM dal INNER JOIN verseny ON dal.ev=verseny.ev";
			MySqlCommand command = new MySqlCommand(queryText, connection);
			MySqlDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				Euroviz uj = new Euroviz(
					reader.GetInt32(0),
					reader.GetString(1),
					reader.GetString(2),
					reader.GetInt32(3),
					reader.GetInt32(4));
				dataList.Add(uj);
			}
			reader.Close();

			dtgkiiratas.ItemsSource = dataList;
		}
		private void AdatbazisLezarasa()
		{

			connection.Close();
			connection.Dispose();
		}

		private void btnFeladat4_Click(object sender, RoutedEventArgs e)
		{
			int magyarVersenyzoCount = GetMagyarVersenyzoCount();
			int legjobbHelyezes = GetLegjobbHelyezes();

			MessageBox.Show("Magyar versenyzők száma: " + magyarVersenyzoCount + "\n" +
							"Legjobb helyezés: " + (legjobbHelyezes == int.MaxValue ? "Nincs adat" : legjobbHelyezes.ToString()),
							"Eredmények");

		}
		private int GetMagyarVersenyzoCount()
		{
			int count = 0;

			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				string queryText = "SELECT COUNT(*) FROM dal WHERE orszag = \"Magyarorszag\"";

				using (MySqlCommand command = new MySqlCommand(queryText, connection))
				{
					count = Convert.ToInt32(command.ExecuteScalar());
				}

				connection.Close();
			}

			return count;
		}
		private int GetLegjobbHelyezes()
		{
			int legjobbHelyezes = int.MaxValue;

			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				string queryText = "SELECT MIN(helyezes) FROM dal WHERE orszag = \"Magyarorszag\"";

				using (MySqlCommand command = new MySqlCommand(queryText, connection))
				{
					object result = command.ExecuteScalar();
					if (result != null && result != DBNull.Value)
					{
						legjobbHelyezes = Convert.ToInt32(result);
					}
				}

				connection.Close();
			}

			return legjobbHelyezes;
		}

		private void btnFeladat5_Click(object sender, RoutedEventArgs e)
		{
			double atlagPontszam = GetNemetorszagAtlagPontszam();
			string eredmeny = atlagPontszam.ToString("N2");

			MessageBox.Show("Németország átlagos pontszáma: " + eredmeny, "Eredmény");
		}

		private double GetNemetorszagAtlagPontszam()
		{
			double atlagPontszam = 0;
			int versenyCount = 0;

			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				string queryText = "SELECT pontszam FROM dal WHERE orszag = \"Nemetorszag\"";

				using (MySqlCommand command = new MySqlCommand(queryText, connection))
				{
					using (MySqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							double pontszam = Convert.ToDouble(reader["pontszam"]);
							atlagPontszam += pontszam;
							versenyCount++;
						}
					}
				}

				connection.Close();
			}

			if (versenyCount > 0)
			{
				atlagPontszam /= versenyCount;
			}

			return Math.Round(atlagPontszam, 2);
		}

		private void btnFeladat6_Click(object sender, RoutedEventArgs e)
		{
			string talalatok = GetLuckDalok();

			if (!string.IsNullOrEmpty(talalatok))
			{
				MessageBox.Show(talalatok, "Találatok");
			}
			else
			{
				MessageBox.Show("Nincsenek találatok.", "Találatok");
			}
		}
		private string GetLuckDalok()
		{
			StringBuilder sb = new StringBuilder();

			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				string queryText = "SELECT eloado, cim FROM dal WHERE cim LIKE \"%Luck%\""; 

				using (MySqlCommand command = new MySqlCommand(queryText, connection))
				{
					using (MySqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string eloado = reader["eloado"].ToString();
							string cim = reader["cim"].ToString();

							sb.Append(eloado + " - " + cim + ", ");
						}
					}
				}

				connection.Close();
			}

			if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 2, 2);
			}

			return sb.ToString();
		}

		private void btnFeladat7_Click(object sender, RoutedEventArgs e)
		{
			string keresettSzoveg = txtEloadoNev.Text;
			List<string> talalatok = GetDalokByElodo(keresettSzoveg);

			lbKiir.Items.Clear();

			foreach (string talalat in talalatok)
			{
				lbKiir.Items.Add(talalat);
			}
		}
		private List<string> GetDalokByElodo(string keresettSzoveg)
		{
			List<string> talalatok = new List<string>();

			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				string queryText = "SELECT eloado, cim FROM dal WHERE eloado LIKE '%" + keresettSzoveg + "%' ORDER BY eloado, cim"; 

				using (MySqlCommand command = new MySqlCommand(queryText, connection))
				{
					using (MySqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string eloado = reader["eloado"].ToString();
							string cim = reader["cim"].ToString();

							string talalat = eloado + " - " + cim;
							talalatok.Add(talalat);
						}
					}
				}

				connection.Close();
			}

			return talalatok;
		}

		private void btnFeladat8_Click(object sender, RoutedEventArgs e)
		{
			string queryText = "SELECT datum FROM verseny";
			MySqlCommand command = new MySqlCommand(queryText, connection);
			string versenyDatum = "";
			List<string> szukitett = new List<string>();

			szukitett.Add();
			using (MySqlDataReader reader = command.ExecuteReader())

			{
				while (reader.Read())
				{

					versenyDatum = Convert.ToString(reader["datum"]);
				}

				lblDatum.Content = "Verseny dátuma: " + versenyDatum;
			}
		}

	}
}

