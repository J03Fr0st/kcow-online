/**
 * Test Data Fixtures for Activities E2E Tests
 *
 * Provides sample activity data for comprehensive CRUD testing.
 * Includes activities with various field combinations to test edge cases.
 */

export interface TestActivity {
  code: string;
  name: string;
  description?: string;
  folder?: string;
  gradeLevel?: string;
  icon?: string | null;
}

/**
 * Test activities covering different scenarios:
 * - Basic activity with minimal required fields
 * - Activity with icon (base64 encoded)
 * - Activity designated for update operations
 * - Activity designated for delete operations
 * - Activity with all fields populated
 */
export const testActivities: Record<string, TestActivity> = {
  basic: {
    code: `E2E-BASIC-${Date.now()}`,
    name: 'E2E Test Activity',
    description: 'A test activity for E2E testing',
    folder: 'TEST',
    gradeLevel: '1',
    icon: null,
  },

  withIcon: {
    code: `E2E-ICON-${Date.now()}`,
    name: 'Activity With Icon',
    description: 'Test activity with icon',
    folder: 'TEST',
    gradeLevel: '2',
    // 1x1 red pixel in base64 PNG format
    icon: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
  },

  forUpdate: {
    code: `E2E-UPDATE-${Date.now()}`,
    name: 'Activity to Update',
    description: 'This will be updated',
    folder: 'UPDATE',
    gradeLevel: '3',
  },

  forDelete: {
    code: `E2E-DELETE-${Date.now()}`,
    name: 'Activity to Delete',
    description: 'This will be deleted',
    folder: 'DELETE',
    gradeLevel: '4',
  },

  complete: {
    code: `E2E-COMPLETE-${Date.now()}`,
    name: 'Complete Test Activity',
    description: 'This activity has all fields populated for comprehensive testing',
    folder: '/Activities/Complete',
    gradeLevel: 'Grade 5-6',
  },

  minimal: {
    code: `E2E-MINIMAL-${Date.now()}`,
    name: 'Minimal Activity',
    // Only required/essential fields
  },
};

/**
 * Helper function to get a fresh test activity with a unique code
 * @param type - The type of test activity to create
 * @returns A test activity object with a unique code
 */
export function createTestActivity(type: keyof typeof testActivities = 'basic'): TestActivity {
  const baseActivity = testActivities[type];
  return {
    ...baseActivity,
    code: `${baseActivity.code}-${Math.random().toString(36).substring(7)}`,
  };
}

/**
 * Helper to get activity code from a test activity object
 */
export function getActivityCode(activity: TestActivity): string {
  return activity.code;
}

/**
 * Helper to get activity name from a test activity object
 */
export function getActivityName(activity: TestActivity): string {
  return activity.name;
}
