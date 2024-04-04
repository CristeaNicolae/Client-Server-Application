using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    private string Nume;
    private string IP;
    public static void Main()
    {
        Console.Write("Nume: ");
        string Nume = Console.ReadLine() + "";

        Client clientDetails = new Client(Nume);

    reconnect:

        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 8080);
            NetworkStream stream = client.GetStream();

            Console.WriteLine("Conectat la server cu succes :)");

            Thread readThread = new Thread(() => ReadMessageFromServer(stream));
            readThread.Start();

            //Thread readClient = new Thread(() => ReadMessageFromClient(stream));
            //readClient.Start();

            while (true)
            {
                Console.Write("Introduceti mesajul: ");
                string message = Console.ReadLine();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);

                if (message.Equals("#exit"))
                {
                    Environment.Exit(0);
                }

            }

            //client.Close();
        }
        catch (IOException)
        {
            Console.WriteLine("Deconectat de la server :(");
            goto reconnect;
            //string readmsg = Console.ReadLine() + "";
            //if (readmsg.Equals("Y") || readmsg.Equals("y")) Environment.Exit(0);
        }
        catch (SocketException)
        {
            Console.WriteLine("Nu s-a putut conecta la server");
            while (true)
            {
                Console.WriteLine("Reconnect? Y/N");
                string readmsg = Console.ReadLine() + "";
                if (readmsg.Equals("N") || readmsg.Equals("n")) Environment.Exit(0);
                else if (readmsg.Equals("Y") || readmsg.Equals("y")) goto reconnect; 
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("A aparut o eroare" + e.StackTrace);
        }
    }

    public static void ReadMessageFromServer(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string message = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("\nServer: " + message + "\n");
                Console.Write("Introduceti mesajul: ");
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("\nConexiune închisă brusc: " + e.Message);
        }
    }

    public static void ReadMessageFromClient(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                Console.Write("Introduceti mesajul: ");
                string message = Console.ReadLine();
                if (message != null)
                {
                    if (message[0] != '#')
                    {
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                    else 
                    { 
                        switch (message)
                        {
                            case "#exit":
                                Environment.Exit(0);
                                break;
                            case "#decc":
                                stream.Close();

                                break;
                            default:
                                break;
                        }
                    }
                }

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("A aparut o eroare" + e.StackTrace);
        }
    }

    public static string GetMachineIP()
    {
        string hostName = Dns.GetHostName();
        IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
        foreach (IPAddress ip in localIPs)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // Verifică dacă este adresă IPv4
            {
                return ip.ToString();
            }
        }
        return "";
    }

    public Client(string nume)
    {
        Nume = nume;
        IP = GetMachineIP();
    }

    public string getNume() { return Nume;}
    public string getIP() { return IP; }
}
