import { environment } from './environment.prod';

describe('Production Environment', () => {
  it('should have production set to true', () => {
    expect(environment.production).toBe(true);
  });

  it('should have correct backend API URL for production', () => {
    expect(environment.apiUrl).toBe('/api');
  });
});
