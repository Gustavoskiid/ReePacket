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
        public event EventHandler<OperationRequestEventArgs> OperationRequest;
        public event EventHandler<OperationResponseEventArgs> OperationResponse;
        public event EventHandler<EventDataEventArgs> EventData;

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
                                OperationRequestEventArgs requestArgs = new OperationRequestEventArgs();
                                requestArgs.OperationCode = requestData.OperationCode;
                                requestArgs.Parameters = requestData.Parameters;
                                OnOperationRequest(requestArgs);
                                break;
                            case 3: // OperationResponse
                                var responseData = protocol16.DeserializeOperationResponse(payload);
                                OperationResponseEventArgs responseArgs = new OperationResponseEventArgs();
                                responseArgs.OperationCode = responseData.OperationCode;
                                responseArgs.ReturnCode = responseData.ReturnCode;
                                responseArgs.Parameters = responseData.Parameters;
                                OnOperationResponse(responseArgs);
                                break;
                            case 4: // EventData
                                var eventData = protocol16.DeserializeEventData(payload);
                                EventDataEventArgs dataArgs = new EventDataEventArgs();
                                dataArgs.Code = eventData.Code;
                                dataArgs.Parameters = eventData.Parameters;
                                OnEventData(dataArgs);
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

        protected virtual void OnOperationRequest(OperationRequestEventArgs e)
        {
            EventHandler<OperationRequestEventArgs> handler = OperationRequest;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnOperationResponse(OperationResponseEventArgs e)
        {
            EventHandler<OperationResponseEventArgs> handler = OperationResponse;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnEventData(EventDataEventArgs e)
        {
            EventHandler<EventDataEventArgs> handler = EventData;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class OperationRequestEventArgs : EventArgs
    {
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters;
    }

    public class OperationResponseEventArgs : EventArgs
    {
        public byte OperationCode { get; set; }
        public short ReturnCode { get; set; }
        public Dictionary<byte, object> Parameters;
    }

    public class EventDataEventArgs : EventArgs
    {
        public byte Code { get; set; }
        public Dictionary<byte, object> Parameters;
    }
}
