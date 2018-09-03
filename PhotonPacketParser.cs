using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReeCode
{
    class PhotonPacketParser
    {
		// Thanks to https://github.com/rafalfigura for the majority of this class
        public void ParsePacket(byte[] photonPacket)
        {
            Protocol16 protocol16 = new Protocol16();
            Stream stream = new MemoryStream(photonPacket);

            BinaryReader reader = new BinaryReader(stream);

            var peerId = IPAddress.NetworkToHostOrder(reader.ReadUInt16());
            var crcEnabled = reader.ReadByte();
            var commandCount = reader.ReadByte();
            var timestamp = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            var challenge = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            var commandHeaderLength = 12;
            var signifierByteLength = 1;

            for (int commandIdx = 0; commandIdx < commandCount; commandIdx++)
            {
                byte commandType = reader.ReadByte();
                byte channelId = reader.ReadByte();
                byte commandFlags = reader.ReadByte();
                byte unkBytes = reader.ReadByte();
                int commandLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                int sequenceNumber = IPAddress.NetworkToHostOrder(reader.ReadInt32());

                switch (commandType)
                {
                    case 4: //Disconnect
                        break;
                    case 7: //Send unreliable
                        reader.BaseStream.Position += 4;
                        commandLength -= 4;
                        goto case 6;
                    case 6: //Send reliable
                        reader.BaseStream.Position += signifierByteLength;
                        byte messageType = reader.ReadByte();
                        int operationLength = commandLength - commandHeaderLength - 2;
                        StreamBuffer payload = new StreamBuffer(reader.ReadBytes(operationLength));
                        switch (messageType)
                        {
                            case 2: // OperationRequest
                                   var requestData = protocol16.DeserializeOperationRequest(payload);
                                   ProcessRequestData(requestData.OperationCode, requestData.Parameters);
                                break;
                            case 3: // OperationResponse
                                var responseData = protocol16.DeserializeOperationResponse(payload);
                                ProcessResponseData(responseData.OperationCode, responseData.ReturnCode, responseData.Parameters);
                                break;
                            case 4: // EventData
                                var eventData = protocol16.DeserializeEventData(payload);
                                ProcessEventData(eventData.Code, eventData.Parameters);
                                break;
                            default:
                                reader.BaseStream.Position += operationLength;
                                break;
                        }
                        break;

                    default:
                        reader.BaseStream.Position += commandLength - commandHeaderLength;
                        break;
                }
            }
        }

        public void ProcessRequestData(byte opCode, Dictionary<byte, object> parameters)
        {
            // Place handler code here
        }

        public void ProcessEventData(byte code, Dictionary<byte, object> parameters)
        {
            // Place handler code here - This is probably the one you want
        }

        public void ProcessResponseData(byte opCode, short returnCode, Dictionary<byte, object> parameters)
        {
            // Place handler code here
        }
    }
}
