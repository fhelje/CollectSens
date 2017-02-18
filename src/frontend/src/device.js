import {bindable} from 'aurelia-framework';
import {HttpClient, json} from 'aurelia-fetch-client';

let client = new HttpClient();

export class Device {
  @bindable value;
  id;
  name = 'Huzzah 1';
  location = 'Köket';
  description = 'En lite längre beskrivning';
  sensorEditable = false;
  interval=10;
  currentTemperature=NaN;
  timestamp;
  myData = {
    labels: ["January", "February", "March", "April", "May", "June", "July"],
    datasets: [
        {
            label: "My First dataset",
            fill: false,
            lineTension: 0.1,
            backgroundColor: "rgba(75,192,192,0.4)",
            borderColor: "rgba(75,192,192,1)",
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: "rgba(75,192,192,1)",
            pointBackgroundColor: "#fff",
            pointBorderWidth: 1,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: "rgba(75,192,192,1)",
            pointHoverBorderColor: "rgba(220,220,220,1)",
            pointHoverBorderWidth: 2,
            pointRadius: 1,
            pointHitRadius: 10,
            data: [65, 59, 80, 81, 56, 55, 40],
            spanGaps: false,
        }
    ]
};

  sensors = [
      {
          type: 'Temperature',
          name: 'Inne'          
      }
  ]
  
  save() {
    let deviceMetadata = 
    {
        deviceId: this.id,
        name: this.name,
        location: this.location,
        description: this.description,
        value: this.value
    }

    client.fetch('http://localhost:5000/api/sensor/' + this.id, {
        method: 'put',
        body: json(deviceMetadata)
    })
  }
  
  loadSensorData() {
      client.fetch('http://localhost:5000/api/sensor/' + this.id )
          .then(response => response.json())
          .then(data => {
              this.name = data.name;
              this.location = data.location;
              this.description = data.description;
          });      
  }

  loadTemperatureHistogram() {
      client.fetch('http://localhost:5000/api/sensor/' + this.id + '/histogram')
          .then(response => response.json())
          .then(data => {
            this.timestamp = data.timestamp;
            this.currentTemperature = data.currentTemperature;
            this.myData = {
                labels: data.labels,
                datasets: [
                    {
                        label: "Temperatur",
                        fill: false,
                        lineTension: 0.1,
                        backgroundColor: "rgba(75,192,192,0.4)",
                        borderColor: "rgba(75,192,192,1)",
                        borderCapStyle: 'butt',
                        borderDash: [],
                        borderDashOffset: 0.0,
                        borderJoinStyle: 'miter',
                        pointBorderColor: "rgba(75,192,192,1)",
                        pointBackgroundColor: "#fff",
                        pointBorderWidth: 1,
                        pointHoverRadius: 5,
                        pointHoverBackgroundColor: "rgba(75,192,192,1)",
                        pointHoverBorderColor: "rgba(220,220,220,1)",
                        pointHoverBorderWidth: 2,
                        pointRadius: 1,
                        pointHitRadius: 10,
                        data: data.values,
                        spanGaps: false,
                    }
                ]
            }
          });
  }

  activate(params, routeConfig) {
      this.id = params.id;
      this.loadSensorData();
      this.loadTemperatureHistogram();
  }
}
