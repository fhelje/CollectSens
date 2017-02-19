import socket

class SensorReporter(object):
    """Do http get request"""

    def __init__(self, device_id, network_adress, port):
        self.device_id = device_id
        self.network_adress = network_adress
        self.port = port
        self.adress = socket.getaddrinfo(network_adress, port)[0][-1]

    def get_http_get_request(self, path):
        """Generate http 1.0 string"""
        data = (path, self.network_adress, self.port)
        return bytes('GET /%s HTTP/1.0\r\nHost: %s:%s\r\n\r\n' % data, 'utf8')

    def get_measurement_url(self, value, sensor_type):
        """ Creates url for measurement reporting"""
        data = (self.network_adress, self.port, self.device_id, sensor_type, value)
        return 'http://%s:%s/api/metric/%s/%s/%s' % data


    def report(self, value, sensor_type):
        """ Report measurement"""
        soc = socket.socket()
        try:
            soc.connect(self.adress)
            soc.send(self.get_http_get_request(self.get_measurement_url(value, sensor_type)))
            while True:
                data = soc.recv(100)
                if data:
                    pass
                else:
                    break
            soc.close()
        except OSError:
            print('Error connecting or sending data')
