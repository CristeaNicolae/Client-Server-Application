using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{

    private static TcpClient client;
    private static NetworkStream stream;

    private static bool stopThread = false;
    public static void Main()
    {
        TryConnection();   
    }

    public static void TryConnection()
    {
        reconnect:
        stopThread = false;
        try
        {
            client = new TcpClient("127.0.0.1", 8080);
            stream = client.GetStream();

            Console.WriteLine("Conectat la server cu succes :)");

            Thread readServer = new Thread(() => ReadMessageFromServer(stream));
            readServer.Start();

            Thread readClient = new Thread(() => ReadMessageFromClient(stream));
            readClient.Start();
        }
        catch (IOException)
        {
            Console.WriteLine("Deconectat de la server :(");
            goto reconnect;
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
                if (message.Length == 0) break;
                Console.WriteLine("\nServer: " + message);
                Console.Write("Introduceti mesajul: ");
            }
        }
        catch (IOException e)
        {
            if(!stopThread) 
                Console.WriteLine("\nConexiune închisă brusc la citirea de la server: " + e.Message);
            Console.Write('\n');
            recon:
            Console.WriteLine("Reconnect? Y/N");
            string readmsg = Console.ReadLine() + "";
            if (readmsg.Equals("N") || readmsg.Equals("n")) Environment.Exit(0);
            else if (readmsg.Equals("Y") || readmsg.Equals("y"))
            {
                stopThread = true;
                TryConnection(); 
            }
            else goto recon;
        }
        catch (Exception e)
        {
            Console.WriteLine("Ceva nu a mers bine (-_-) " + e.StackTrace);
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

                if (!string.IsNullOrEmpty(message))
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
                                stopThread = true;
                                stream.Close();
                                break;
                            default:
                                Console.WriteLine("comanda invalida");
                                break;
                        }
                    }
                }

            }
        }
        catch (IOException e)
        {
            Console.WriteLine("\nConexiune închisă brusc la citirea de la consola: " + e.Message);
            recon:
            Console.WriteLine("Reconnect? Y/N");
            string readmsg = Console.ReadLine() + "";
            if (readmsg.Equals("N") || readmsg.Equals("n")) Environment.Exit(0);
            else if (readmsg.Equals("Y") || readmsg.Equals("y"))
            {
                stopThread = true;
                TryConnection();
            }
            else goto recon;
        }
        catch (Exception e)
        {
            if(!stopThread)
                Console.WriteLine("A aparut o eroare la trimiterea mesajului" + e.StackTrace);
        }
    }

}
