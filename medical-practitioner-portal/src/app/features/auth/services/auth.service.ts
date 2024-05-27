import { Injectable } from '@angular/core';

import { Observable, from } from 'rxjs';

import { KeycloakService } from 'keycloak-angular';
import { KeycloakLoginOptions } from 'keycloak-js';

export interface IAuthService {
  login(options?: KeycloakLoginOptions): Observable<void>;
  isLoggedIn(): Observable<boolean>;
  logout(redirectUri: string): Observable<void>;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService implements IAuthService {
  public constructor(private keycloakService: KeycloakService) {}

  public login(options?: KeycloakLoginOptions): Observable<void> {
    return from(this.keycloakService.login(options));
  }

  public getHpdid(): string | undefined {
    return this.keycloakService.getKeycloakInstance()?.idTokenParsed?.sub;
      // Stanley's POC used this but I would think it would be "sid"
      //.preferred_username;
  }

  public isLoggedIn(): Observable<boolean> {
    return from(this.keycloakService.isLoggedIn());
  }

  public logout(redirectUri: string): Observable<void> {
    return from(this.keycloakService.logout(redirectUri));
  }
}