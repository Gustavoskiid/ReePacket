# ReePacket
This project is really just a collection of class files that contain a basic implementation for parsing raw network byte data into a more user-friendly format for purposes of analysis.

## Sample Usage

```cs
static void Sniff(string ip)
{
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
            // You know from elsewhere that this is Unity Photon Data
            // You now need PhotonPacketParser.cs as well as the referenced DLL
            PhotonPacketParser ppp = new PhotonPacketParser();
            ppp.ParsePacket(p.Payload); // You will need to edit PhotonPacketParser.cs to deal with this stuff
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
