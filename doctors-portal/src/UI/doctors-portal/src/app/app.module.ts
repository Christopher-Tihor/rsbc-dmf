import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LayoutModule } from '@angular/cdk/layout';
import { SharedModule } from './shared/shared.module';
import { LayoutModule as PortalLayoutModule } from './layout/layout.module';
import { ApiModule } from './shared/api/api.module';
import { HttpClientModule } from '@angular/common/http';
import { OAuthModule } from 'angular-oauth2-oidc';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,
    SharedModule,
    PortalLayoutModule,
    HttpClientModule,
    ApiModule.forRoot({ rootUrl: '' }),
    OAuthModule.forRoot({
      resourceServer: {
        sendAccessToken: true,
        customUrlValidation: url => url.toLowerCase().startsWith('/api') && !url.toLowerCase().endsWith('/config'),
      }
    })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
