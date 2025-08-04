import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { ErrorInterceptor } from './main/error.interceptor'

import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import { TFileComponent } from './tfile/tfile.component';
import {FormsModule} from "@angular/forms";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MainComponent } from './main/main.component';
@NgModule({
  declarations: [
    AppComponent,
    TFileComponent,
    MainComponent

  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    BrowserAnimationsModule
  ],

  providers: [
    HttpClientModule,
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

  ],
  bootstrap: [MainComponent]
})
export class AppModule { }


export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

