import environment from './environment';
import chart from 'chart.js'

//Configure Bluebird Promises.
Promise.config({
  warnings: {
    wForgottenReturn: false
  }
});

export function configure(aurelia) {
  aurelia.use
    .standardConfiguration()
    .feature('resources');

  if (environment.debug) {
    aurelia.use.developmentLogging();
  }

  if (environment.testing) {
    aurelia.use.plugin('aurelia-testing');
  }
  aurelia.use.plugin("aurelia-chart");
  aurelia.start().then(() => aurelia.setRoot());
  chart.defaults.global.maintainAspectRatio = false;
}
