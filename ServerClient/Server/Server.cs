using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static List<TcpClient> clients = new List<TcpClient>();

    private static bool stopWritingClientDecon = false;

    public static void Main()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8080;
        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        Console.WriteLine("Serverul este pornit...");

        Thread consoleThread = new Thread(ReadConsoleAndBroadcast);
        consoleThread.Start();

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine("\nClient conectat. :)");
            Console.Write("Server: ");

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    public static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        try
        {
            while (true)
            {
                if (client.Connected)
                {
                    byte[] data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);
                    string message = Encoding.ASCII.GetString(data, 0, bytes);
                    if (!String.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("\nClient: " + message);
                        Console.Write("Server: ");
                        BroadcastMessage(message, client);
                    }
                }
                else break;

            }
        }
        catch (IOException e)
        {
            if (!stopWritingClientDecon)
            { 
                Console.WriteLine("\nclient deconectat :(");
                Console.Write("Server: ");
                clients.Remove(client);
            }
            else stopWritingClientDecon = true;
        }
        catch(Exception e)
        {
            Console.WriteLine("A aparut o eroare " + e.StackTrace);
        }
        finally
        {
            clients.Remove(client);  // inlatura clientul din lista în orice caz 
            client.Close();
        }

        client.Close();
    }

    public static void ReadConsoleAndBroadcast()
    {
        retry:
        try 
        {
            while (true)
            {
                Console.Write("Server: ");
                string message = Console.ReadLine() + "";
                if (!string.IsNullOrEmpty(message)) 
                { 
                    if (message[0] != '#')
                    {
                        BroadcastMessage(message, null);
                    }
                    else HandleCommand(message); 
                }
            } 
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine("Nu s-a putut transmite mesajul" + e.StackTrace);
            goto retry;
        }
    }

    public static void BroadcastMessage(string message, TcpClient? client)  //Transmite mesajul unui client/serverului mai departe
    {
        int i = clients.Count - 1;
        if (client != null)
        { 
            while (i >= 0)
            {
                TcpClient clientAux = clients[i];
                if (clientAux != client)
                {
                    if (clientAux.Connected)
                    {
                        NetworkStream stream = clientAux.GetStream();
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        clients.RemoveAt(i);
                    }
                }
                i--;
            }
        }
        else
        {
            while (i >= 0)
            {
                TcpClient client1 = clients[i];
                if (client1.Connected)
                {
                    NetworkStream stream = client1.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    clients.RemoveAt(i);
                }
                i--;
            }
        }
    }

    public static void HandleCommand(string mes)
    {
        switch(mes)
        {
            case "#exit":                                                      //inchide consola si programul
                Environment.Exit(0);
                break;
            case "#ccount":                                                     //afiseaza numarul de clienti conectati
                Console.WriteLine("Numarul de clienti: " + clients.Count);
                break;
            case "#decc":                                                      //deconecteaza toti clientii
                int i = clients.Count - 1;
                while(i >= 0) 
                { 
                    clients[i].Dispose();
                    clients.RemoveAt(i);
                    i--;
                }
                if (clients.Count == 0) Console.WriteLine("toti clientii au fost deconectati cu succes");
                else Console.WriteLine("a aparut o eroare la deconectarea clientilor");
                stopWritingClientDecon = true;
                break;
            default:
                Console.WriteLine("comanda invalida");
                break;
        }
    }
}
