import { makeEnvironmentProviders } from '@angular/core';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../../environments/environment';

export function provideApi() {
  return makeEnvironmentProviders([
    {
      provide: ApiConfiguration,
      useFactory: () => {
        const config = new ApiConfiguration();
        config.rootUrl = environment.apiUrl;
        return config;
      },
    },
  ]);
}
