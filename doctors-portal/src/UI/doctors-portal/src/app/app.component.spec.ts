import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app.component';
import { LoginService } from './shared/services/login.service';
import { LoginStubService } from './shared/stubs/login.service.stub';
import { ConfigurationService } from './shared/services/configuration.service';
import { ConfigurationStubService } from './shared/stubs/configuration.service.stub';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        HttpClientTestingModule
      ], providers: [
        { provide: ConfigurationService, useClass: ConfigurationStubService },
        { provide: LoginService, useClass: LoginStubService }
      ],
      declarations: [
        AppComponent
      ],
    }).compileComponents();
  });

  // it('should create the app', () => {
  //   const fixture = TestBed.createComponent(AppComponent);
  //   const app = fixture.componentInstance;
  //   expect(app).toBeTruthy();
  // });

});
