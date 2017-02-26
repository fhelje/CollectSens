import nativeio
import bitbangio
from board import *
import network
import time
import esp
import network
from adafruit_mcp9808 import MCP9808
from sensorReporter import SensorReporter

INTERVAL = 10
TEMPERATURE_SENSOR_TYPE = 0

def get_device_id():
    """ GEt unique id for device"""
    flash_id = '{0:x}'.format(esp.flash_id())
    manufacturer = flash_id[-2:]
    device_id = flash_id[2:4] + flash_id[0:2]
    return (manufacturer, device_id)

def get_configuration():
    """Read configuration from config.yml"""
    return ('192.168.1.198', 5000)

def do_connect():
    """ Connect to network and reconnect if not already connected"""

    sta_if = network.WLAN(network.STA_IF)
    if not sta_if.isconnected():
        print('connecting to network...')
        sta_if.active(True)
        sta_if.connect('heljebrandt', 'aabbccddee')
        while not sta_if.isconnected():
            pass
    print('network config:', sta_if.ifconfig())

def start(sensor, led):
    """Main loop"""

    _, device_id = get_device_id()
    adress, port = get_configuration()

    temp_sensor = MCP9808(sensor)
    reporter = SensorReporter(device_id, adress, port)

    print('Starting temperature logger')
    print('---------------------------')
    print('Device id: ', device_id)
    print('Log to', adress, ':', port)

    while True:
        led.value = 1
        print(temp_sensor.temperature, " C")
        reporter.report(temp_sensor.temperature, TEMPERATURE_SENSOR_TYPE)
        reporter.get_interval()
        led.value = 0
        time.sleep(INTERVAL)

def run():
    """main"""
    do_connect()

    with bitbangio.I2C(SCL, SDA) as i2c, nativeio.DigitalInOut(GPIO15) as pin:
        pin.switch_to_output()
        start(i2c, pin)
