using SimpleJSON;
using ROSBridgeLib.std_msgs;
using System;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class BatteryStateMsg : ROSBridgeMsg
        {
            private HeaderMsg _header;
            private float _capacity;
            private float _charge;
            private float _current;
            private float _design_capacity;
            private float _percentage;
            private float _voltage;
            private float[] _cell_voltage;
            private string _location;
            private uint _power_supply_health;
            private uint _power_supply_status;
            private uint _power_supply_technology;
            private bool _present;
            private string _serial_number;


            public BatteryStateMsg(JSONNode msg)
            {
                _header = new HeaderMsg(msg["header"]);
                if (!msg["capacity"].Value.Equals("null"))
                    _capacity = float.Parse(msg["capacity"], System.Globalization.CultureInfo.InvariantCulture);
                if(!msg["charge"].Value.Equals("null"))
                    _charge = float.Parse(msg["charge"], System.Globalization.CultureInfo.InvariantCulture);
                _current = float.Parse(msg["current"], System.Globalization.CultureInfo.InvariantCulture);
                _design_capacity = float.Parse(msg["design_capacity"], System.Globalization.CultureInfo.InvariantCulture);
                if(!msg["percentage"].Value.Equals("null"))
                    _percentage = float.Parse(msg["percentage"], System.Globalization.CultureInfo.InvariantCulture);
                _voltage = float.Parse(msg["voltage"], System.Globalization.CultureInfo.InvariantCulture);

                _cell_voltage = new float[msg["cell_voltage"].Count];
                for (int i = 0; i < _cell_voltage.Length; i++)
                {
                    _cell_voltage[i] = float.Parse(msg["cell_voltage"][i], System.Globalization.CultureInfo.InvariantCulture);
                }
                _location = msg["location"];
                _power_supply_health = uint.Parse(msg["power_supply_health"]);
                _power_supply_status = uint.Parse(msg["power_supply_status"]);
                _power_supply_technology = uint.Parse(msg["power_supply_technology"]);
                _present = bool.Parse(msg["present"]);
                _serial_number = msg["serial_number"];
            }

            public BatteryStateMsg(HeaderMsg header, float capacity, float charge, float current, float design_capacity, float percentage, float voltage, float[] cell_voltage, string location, uint power_supply_health, uint power_supply_status, uint power_supply_technology,bool present,string serial_number)
            {
                _header = header;
                _capacity = capacity;
                _charge = charge;
                _current = current;
                _design_capacity = design_capacity;
                _percentage = percentage;
                _voltage = voltage;
                _cell_voltage = cell_voltage;
                _location = location;
                _power_supply_health = power_supply_health;
                _power_supply_status = power_supply_status;
                _power_supply_technology = power_supply_technology;
                _present = present;
                _serial_number = serial_number;
            }

            public static string GetMessageType()
            {
                return "sensor_msgs/BatteryState";
            }

            public HeaderMsg GetHeader() { return _header; }
            public float GetVoltage() { return _voltage; }
            public float GetCurrent() { return _current; }
            public uint GetPowerSupplyStatus() { return _power_supply_status; }

            public override string ToString()
            {
                string resultado = "BatteryState [header=" + _header.ToString()
                                + ",  capacity=" + _capacity.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  charge=" + _charge.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  current=" + _current.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  design_capacity=" + _design_capacity.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  percentage=" + _percentage.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  voltage=" + _voltage.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                                + ",  cell_voltage=[";
                                    for (int i = 0; i < _cell_voltage.Length; i++)
                                    {
                                        resultado += _cell_voltage[i].ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                                        if (i < (_cell_voltage.Length - 1))
                                            resultado += ",";
                                    }
                                    resultado += "]";

                        resultado += ",  location=" + _location
                                    + ", power_supply_health=" + _power_supply_health.ToString()
                                    + ", power_supply_status=" + _power_supply_status.ToString()
                                    + ", power_supply_technology=" + _power_supply_technology.ToString()
                                    + ", present=" + _present.ToString()
                                    + ", serial_number=" + _serial_number + "]";

                return resultado;
            }

            public override string ToYAMLString()
            {
                string resultado = "{\"header\" : " + _header.ToYAMLString()
                + ", \"capacity\" : " + _capacity.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"charge\" : " + _charge.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"current\" : " + _current.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"design_capacity\" : " + _design_capacity.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"percentage\" : " + _percentage.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"voltage\" : " + _voltage.ToString("N", System.Globalization.CultureInfo.InvariantCulture)
                + ", \"cell_voltage\" : [";
                for (int i = 0; i < _cell_voltage.Length; i++)
                {
                    resultado += _cell_voltage[i].ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                    if (i < (_cell_voltage.Length - 1))
                        resultado += ",";
                }
                resultado += "]";

                resultado += ", \"location\" : " + _location
                            + ", \"power_supply_health\" : " + _power_supply_health.ToString()
                            + ", \"power_supply_status\" : " + _power_supply_status.ToString()
                            + ", \"power_supply_technology\" : " + _power_supply_technology.ToString()
                            + ", \"present\" : " + _present.ToString()
                            + ", \"serial_number\" : " + _serial_number + "}";

                return resultado;

            }
        }
    }
}
