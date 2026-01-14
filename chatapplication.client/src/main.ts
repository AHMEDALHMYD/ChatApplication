import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app/app.component';
import { routes } from './app/app-routing.module';

import { provideHttpClient, HTTP_INTERCEPTORS, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './app/interceptors/auth.interceptor';

bootstrapApplication(AppComponent,
  {
    providers: [
      provideHttpClient(
        withInterceptors([authInterceptor])
      ),
      provideRouter(routes)
    ]
  }
).catch(err => console.error(err));
