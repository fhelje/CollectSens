from machine import I2C, Pin, WDT
import network
import time
import esp
from adafruit_mcp9808 import MCP9808
from sensorReporter import SensorReporter

INTERVAL = 10000
TEMPERATURE_SENSOR_TYPE = 0


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

def get_device_id():
    """ GEt unique id for device"""
    flash_id = '{0:x}'.format(esp.flash_id())
    manufacturer = flash_id[-2:]
    device_id = flash_id[2:4] + flash_id[0:2]
    return (manufacturer, device_id)

def get_configuration():
    """Read configuration from config.yml"""
    return ('192.168.1.198', 5000)


def main(sensor, led, watchdog):
    """Main loop"""

    _, device_id = get_device_id()
    adress, port = get_configuration()

    temp_sensor = MCP9808(sensor)
    reporter = SensorReporter(device_id, adress, port)
    do_connect()

    while True:
        time.sleep(INTERVAL)
        led.high()
        reporter.report(temp_sensor.temperature, TEMPERATURE_SENSOR_TYPE)
        led.low()
        # Feed the dog or we will restart
        watchdog.feed()

with I2C(-1, Pin(5), Pin(4)) as i2c, \
     Pin(15, Pin.OUT) as pin, \
     WDT(timeout=INTERVAL*2) as dog:
    main(i2c, pin, dog)
