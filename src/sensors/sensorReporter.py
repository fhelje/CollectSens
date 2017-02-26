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
        return 'GET /%s HTTP/1.0\r\nHost: %s:%s\r\n\r\n' % data

    def get_measurement_url(self, value, sensor_type):
        """ Creates url for measurement reporting"""
        data = (self.device_id, sensor_type, value)
        return 'api/metric/%s/%s/%s' % data

    def get_interval_url(self):
        """ Creates url for updating interval"""
        return 'api/sensor/%s/interval' % self.device_id


    def get_interval(self):
        """ Report measurement"""
        soc = socket.socket()
        response = ''
        try:
            print('Opening socket to adress: ', self.adress)
            soc.connect(self.adress)
            path = self.get_interval_url()
            request = self.get_http_get_request(path)
            soc.send(bytes(request, 'utf8'))
            while True:
                data = soc.recv(100)
                if data:
                    response += str(data, 'utf8')
                else:
                    break
            soc.close()
            print(response)
        except OSError:
            print('Error connecting or sending data')

        print('interval exiting')
        

    def report(self, value, sensor_type):
        """ Report measurement"""
        soc = socket.socket()
        try:
            print('Opening socket to adress: ', self.adress)
            soc.connect(self.adress)
            path = self.get_measurement_url(value, sensor_type)
            request = self.get_http_get_request(path)
            soc.send(bytes(request, 'utf8'))
            while True:
                data = soc.recv(100)
                if data:
                    pass
                else:
                    break
            soc.close()
        except OSError:
            print('Error connecting or sending data')

        print('report exiting')

