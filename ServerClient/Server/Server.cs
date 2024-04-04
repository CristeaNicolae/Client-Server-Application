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
            Console.WriteLine("Client conectat. :)");

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
                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string message = Encoding.ASCII.GetString(data, 0, bytes);
                if (message[0] != '#')
                {
                    Console.WriteLine("Client: " + message);
                    Console.Write("Server: ");
                    BroadcastMessage2(message, client);
                }
                else if (message.Equals("#exit"))
                    {
                        clients.Remove(client);
                        Console.WriteLine("client deconectat :(");
                        break;
                    }
                    
                
            }
        }
        catch(IOException e)
        {
            Console.WriteLine("client deconectat :(");
            clients.Remove(client);
        }
        catch(Exception e)
        {
            Console.WriteLine("A aparut o eroare " + e.StackTrace);
        }

        client.Close();
    }

    public static void BroadcastMessage(string message) //Transmite un mesaj catre toti clientii
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            TcpClient client = clients[i];
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            else
            {
                clients.RemoveAt(i);
            }
        }
    }

    public static void ReadConsoleAndBroadcast()
    {
        retryReadingClient:
        try 
        {
            while (true)
            {
                Console.Write("Server: ");
                string message = Console.ReadLine();
                if (message != null) 
                { 
                    if (message[0] != '#')
                    {
                        BroadcastMessage("" + message);
                    }
                    else HandleCommand(message); 
                }
            } 
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine("Nu s-a putut transmite mesajul" + e.StackTrace);
            goto retryReadingClient;
        }
    }

    public static void BroadcastMessage2(string message, TcpClient client)  //Transmite mesajul unui client mai departe
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            TcpClient clientAux = clients[i];
            if (clientAux.Connected && clientAux != client)
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
    }

    public static void HandleCommand(string mes)
    {
        switch(mes)
        {
            case "#close":                                                      //inchide consola si programul
                Environment.Exit(0);
                break;
            case "#ccount":                                                     //afiseaza numarul de clienti conectati
                Console.WriteLine("Numarul de clienti: " + clients.Count);
                break;
            case "#decon":                                                      //deconecteaza toti clientii                                                            
                for (int i = clients.Count - 1; i >= 0; i--) 
                { 
                    clients[i].Dispose();
                    clients.RemoveAt(i);
                }
                if (clients.Count == 0) Console.WriteLine("toti clientii au fost deconectati cu succes");
                else Console.WriteLine("a aparut o eroare la deconectarea clientilor");
                break;
            default:
                Console.WriteLine("comanda invalida");
                break;
        }
    }
}
