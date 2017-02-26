import machine
import time
import ntptime
import network
import esp
import socket
import utime
from adafruit_mcp9808 import MCP9808


#setup
#  read file from fs
#  network_name: <NAME OF NETWORK>
#  network_password: <PASSWORD>
#  MeasurementApi: <ROOT URL>

def temp_c(data):
    value = data[0] << 8 | data[1]
    temp = (value & 0xFFF) / 16.0
    if value & 0x1000:
        temp -= 256.0
    return temp


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


def http_get(url):
   #try catch retry x times with 10s sleep in between
   _, _, host, path = url.split('/', 3)
   do_connect()
   s = socket.socket()
   print('Http get to: 192.168.1.198:5000')   
   s.connect(('192.168.1.198', 5000))
   s.send(bytes('GET /%s HTTP/1.0\r\nHost: 192.168.1.198:5000\r\n\r\n' % path, 'utf8'))
   while True:
       data = s.recv(100)
       if data:
           pass
       else:
           break
   s.close()
   print('Data sent')   
 
 
def gettime():
    suceeded = False
    while suceeded == False: 
        try:
            tm = utime.localtime(ntptime.time())
            print('ntp', tm)
            tm = tm[0:3] + (0,) + tm[3:6] + (0,)
            machine.RTC().datetime(tm)
            return utime.localtime()
        except:
            time.sleep(1)
            pass
 
 
def getUrlEncodedDate():
       #Needs a try catch
   (year, month, day, hour, minute, second, x, y) = gettime()
   return '{0}-{1}-{2}%20{3}%3A{4}%3A{5}'.format(year, month, day, hour, minute, second)
    #print('Sending')    
    
 
id = '{0:x}'.format(esp.flash_id())
manufacturer = id[-2:]
device_id = id[2:4] + id[0:2]
 
print('Starting device: ', device_id)
 
do_connect()
 
i2c = machine.I2C(-1, machine.Pin(5), machine.Pin(4))
led = machine.Pin(15, machine.Pin.OUT)
address = 24
temp_reg = 5
res_reg = 8
 
data = bytearray(2)
 
while True:
    led.high()
    i2c.readfrom_mem_into(address, temp_reg, data)
    t = temp_c(data)
    print('Temp: ', '{0:.1f} C'.format(t))
    url= 'http://192.168.1.198/api/metric/{0}/0/{1}/{2}'.format(device_id, t, getUrlEncodedDate())
    try:
        http_get(url)
    except:
        time.sleep(1)
        print('error connecting')
    led.low()
    time.sleep(10)