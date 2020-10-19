import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { MainComponent } from './main/main.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule} from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { HttpClientModule } from '@angular/common/http';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { MatSidenavModule } from '@angular/material/sidenav';

import { UploadCertificateComponent } from './upload-certificate/upload-certificate.component';
import { StaticInfoService } from './static-info.service';
import { DevOpsServiceClient } from './service-client.service';
import { EnterPassphraseComponent } from './upload-certificate/enter-passphrase/enter-passphrase.component';
import { ConfiguredPluginComponent } from './main/configured-plugin/configured-plugin.component';
import { SelectAccountsComponent } from './select-accounts/select-accounts.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    MainComponent,
    UploadCertificateComponent,
    EnterPassphraseComponent,
    ConfiguredPluginComponent,
    SelectAccountsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ClipboardModule,
    MatInputModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    MatDialogModule,
    MatExpansionModule,
    MatIconModule,
    MatSidenavModule
  ],
  entryComponents: [
    UploadCertificateComponent
  ],
  providers: [ DevOpsServiceClient, StaticInfoService, { provide: Window, useValue: window } ],
  bootstrap: [ AppComponent ]
})
export class AppModule { }