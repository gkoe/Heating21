using Base.Helper;

using Core.Entities;

using IotServices.DataTransferObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IotServices.Services.MqttAdapter
{
    public class EspSensorBoxMqttAdapter
    {
        record MqttPayload(long Timestamp, double Value);

        public static MeasurementTimeValue? ConvertMqttToMeasurementDto(string topic, string jsonPayload)
        {

            // nl_school/temperature/state
            // {"timestamp":1646408172,"value":16.81}

            topic = topic[0..^6];  // state wegschneiden
            int lastSlashPos = topic.LastIndexOf('/');  // letzter String ist Sensorname
            string itemBaseName = topic[..lastSlashPos];
            if (itemBaseName.Where(ch => ch == ':').Count() == 5)  // Mac-Adresse ==> Thingname ist Miflora und nur Macadresse
            {
                itemBaseName = itemBaseName[^17..];  // letzte 17 Zeichen bleiben (Macadresse)
            }
            string itemDetailName = topic.Substring(lastSlashPos + 1, topic.Length - lastSlashPos - 1);
            string itemName = itemBaseName + "_" + itemDetailName;
            if(!Enum.TryParse<ItemEnum>(itemName, true, out ItemEnum itemEnum))
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var payload = JsonSerializer.Deserialize<MqttPayload>(jsonPayload, options);


            var dto = new MeasurementTimeValue
            {
                ItemEnum = itemEnum,
                Value = payload?.Value ?? 0,
                Time = DateTimeHelpers.UnixTimeStampToDateTime(payload?.Timestamp ?? 0)

            };
            return dto;
        }
    }
}
