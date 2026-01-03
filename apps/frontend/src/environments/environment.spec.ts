import { environment } from './environment';

describe('Development Environment', () => {
  it('should have production set to false', () => {
    expect(environment.production).toBe(false);
  });

  it('should have correct backend API URL for development', () => {
    expect(environment.apiUrl).toBe('http://localhost:5039/api');
  });
});
