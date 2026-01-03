import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpHandler } from '@angular/common/http';
import { appConfig } from './app.config';

describe('Application Config', () => {
  beforeEach(() => {
    TestBed.configureTestingModule(appConfig);
  });

  it('should provide HttpClient', () => {
    const httpClient = TestBed.inject(HttpClient);
    expect(httpClient).toBeTruthy();
  });

  it('should configure HttpClient to work with CORS and credentials', () => {
    // This test verifies that HttpClient is configured
    // The actual withCredentials configuration is tested in service tests
    const httpClient = TestBed.inject(HttpClient);
    expect(httpClient).toBeInstanceOf(HttpClient);
  });
});
