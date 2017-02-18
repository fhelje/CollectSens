import {HttpClient} from 'aurelia-fetch-client';

let client = new HttpClient();

  
export class App {
  sensors = [];
  constructor() {
    this.message = 'Sensor configuration';
    client.fetch('http://localhost:5000/api/devices')
          .then(response => response.json())
          .then(data => {
            this.sensors = data;
          });
  }

  configureRouter(config, router) {
    this.router = router;
    config.title = 'Device configurator';
    config.map([
      { route: ['', 'devices'], name: 'devices',     moduleId: 'devices' },
      { route: 'device/:id',    name: 'device',      moduleId: 'device' }
    ]);
  }
}
