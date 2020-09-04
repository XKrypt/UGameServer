using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>Sent from server to client.</summary>
public enum ServerPackets
{
    welcome = 1
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1
}

public class Packet : IDisposable
{
    byte[] package;
    List<byte> _package = new List<byte>();
    public Packet(byte[] data)
    {
        package = data;
    }

    public Packet(string data)
    {
        byte[] lenght = BitConverter.GetBytes(Encoding.ASCII.GetBytes(data).Length);

        List<byte> messageSender = new List<byte>();

        messageSender.AddRange(lenght);
        messageSender.AddRange(Encoding.UTF8.GetBytes(data));
        _package.AddRange(messageSender);
        Console.WriteLine($"Package lenght is { _package.Count }");
    }

    public int PackageLenght()
    {
        byte[] lenght = new byte[4];
        Array.Copy(package, lenght, 4);
        int _lenght = BitConverter.ToInt32(lenght);
        Console.WriteLine($"Package lenght is { _lenght }");
        return _lenght;
    }

    public string GetData()
    {
        byte[] data = new byte[PackageLenght()];
        Array.Copy(package, 4, data, 0, PackageLenght());
        string datareceived = Encoding.UTF8.GetString(data);

        return datareceived;
    }

    public byte[] writeData()
    {
        return _package.ToArray();
    }

    public void Dispose()
    {
        
        GC.SuppressFinalize(this);
    }
}