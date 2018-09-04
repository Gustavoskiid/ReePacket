# ReePacket
This project is really just a collection of class files that contain a basic implementation for parsing raw network byte data into a more user-friendly format for purposes of analysis.

## Sample Usage

```cs
static void Sniff(string ip)
{
    // You know from elsewhere that you're dealing with Unity Photon Data
    // You now need PhotonPacketParser.cs as well as the referenced DLL
    PhotonPacketParser ppp = new PhotonPacketParser();
    ppp.EventData += ParsePhotonEventData;
    // ppp.OperationRequest
    // ppp.OperationResponse
    
    // http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx
    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
    s.Bind(new IPEndPoint(IPAddress.Parse(ip), 0));
    s.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), null);

    byte[] byteData = new byte[65535];


    // Async methods for recieving and processing data
    Action<IAsyncResult> r = null;
    r = (ar) =>
    {
        ReePacket p = new ReePacket(byteData);
        #region UDP
        if (p.ProtocolType == ProtocolType.Udp) // UDP
        {
            // If any relevant data is parsed, it will be caught by the 3 event handlers assigned above
            ppp.ParsePacket(p.Payload);
        }
        
        //clean out our buffer
        b = new byte[65535];

        //listen some more
        s.BeginReceive(b, 0, 65535, SocketFlags.None, new AsyncCallback(r), null);
    };

    // begin listening to the socket
    s.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(r), null);
    Console.WriteLine("Listening...");
}

public static void ParsePhotonEventData(object sender, EventDataEventArgs data)
{
    // Albion Online Sample
    // This code is simply here for a usage example and should be replaced
    if (data.Code == 2)
    {
        // Console.WriteLine("Player Data!");
        return;
    }
    data.Parameters.TryGetValue(252, out object val); // Referenced in Albion.PhotonClient.dll
    if (val == null) return;
    Console.WriteLine("Albion Online Event id: " + int.Parse(val.ToString()) + " -- " + data.Parameters.Count);
}
