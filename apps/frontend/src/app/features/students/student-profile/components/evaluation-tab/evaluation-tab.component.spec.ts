import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { EvaluationTabComponent } from './evaluation-tab.component';
import { EvaluationService } from '@core/services/evaluation.service';
import { ActivityService } from '@core/services/activity.service';
import { NotificationService } from '@core/services/notification.service';
import { DestroyRef } from '@angular/core';
import type { Evaluation } from '@features/evaluations/models/evaluation.model';
import type { Activity } from '@features/activities/models/activity.model';

describe('EvaluationTabComponent', () => {
  let component: EvaluationTabComponent;
  let fixture: ComponentFixture<EvaluationTabComponent>;
  let evaluationServiceSpy: any;
  let activityServiceSpy: any;
  let notificationServiceSpy: any;

  const mockEvaluationRecords: Evaluation[] = [
    {
      id: 1,
      studentId: 1,
      activityId: 1,
      activityName: 'Reading Level 1',
      evaluationDate: '2026-01-02',
      score: 85,
      speedMetric: 45,
      accuracyMetric: 92,
      notes: 'Good progress',
      createdAt: '2026-01-02T10:00:00Z',
    },
    {
      id: 2,
      studentId: 1,
      activityId: 2,
      activityName: 'Math Basics',
      evaluationDate: '2026-01-01',
      score: 78,
      speedMetric: null,
      accuracyMetric: 88,
      notes: null,
      createdAt: '2026-01-01T10:00:00Z',
    },
  ];

  const mockActivities: Activity[] = [
    {
      id: 1,
      code: 'READ-L1',
      name: 'Reading Level 1',
      description: 'Basic reading comprehension',
      folder: 'Reading',
      gradeLevel: 'Grade 1',
      icon: '📚',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
    },
    {
      id: 2,
      code: 'MATH-BAS',
      name: 'Math Basics',
      description: 'Basic mathematics',
      folder: 'Math',
      gradeLevel: 'Grade 1',
      icon: '🔢',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
    },
  ];

  beforeEach(async () => {
    evaluationServiceSpy = {
      getEvaluations: jest.fn(),
      updateEvaluation: jest.fn(),
      createEvaluation: jest.fn(),
    };

    activityServiceSpy = {
      loadActivities: jest.fn(),
      activities: (() => mockActivities) as any,
      loading: (() => false) as any,
    };

    notificationServiceSpy = {
      success: jest.fn(),
      error: jest.fn(),
      notifications$: (() => []) as any,
    };

    await TestBed.configureTestingModule({
      imports: [EvaluationTabComponent],
      providers: [
        { provide: EvaluationService, useValue: evaluationServiceSpy },
        { provide: ActivityService, useValue: activityServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: HttpClient, useValue: {} },
        { provide: DestroyRef, useValue: { destroy: () => {} } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EvaluationTabComponent);
    component = fixture.componentInstance;

    // Set the required input signal
    (component as any).studentId = () => 1; // Make it act as a signal function
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Loading evaluation records', () => {
    beforeEach(() => {
      (component as any).studentId = () => 1; // Ensure studentId signal is set
    });

    it('should load evaluation records on init', () => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));

      component.ngOnInit();
      fixture.detectChanges();

      expect(evaluationServiceSpy.getEvaluations).toHaveBeenCalledWith(1);
      expect(component.evaluationRecords()).toEqual(mockEvaluationRecords);
      expect(component.isLoading()).toBe(false);
    });

    it('should handle loading error', () => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(throwError(() => new Error('Error')));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.evaluationRecords()).toEqual([]);
      expect(component.isLoading()).toBe(false);
      expect(component.error()).toBe('Failed to load evaluation records');
    });

    it('should handle empty response', () => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.evaluationRecords()).toEqual([]);
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Loading activities', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
    });

    it('should load activities on init', () => {
      component.ngOnInit();
      fixture.detectChanges();

      expect(activityServiceSpy.loadActivities).toHaveBeenCalled();
      expect(component.activities()).toEqual(mockActivities);
    });
  });

  describe('Sorting records', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should sort records by date descending', () => {
      const sorted = component.sortedRecords();
      expect(sorted[0].evaluationDate).toBe('2026-01-02');
      expect(sorted[1].evaluationDate).toBe('2026-01-01');
    });
  });

  describe('Score styling', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return success class for score >= 80', () => {
      expect(component.getScoreClass(85)).toBe('badge-success');
    });

    it('should return warning class for score >= 60', () => {
      expect(component.getScoreClass(65)).toBe('badge-warning');
    });

    it('should return error class for score < 60', () => {
      expect(component.getScoreClass(45)).toBe('badge-error');
    });

    it('should return ghost class for undefined score', () => {
      expect(component.getScoreClass(undefined)).toBe('badge-ghost');
    });

    it('should return correct score level', () => {
      expect(component.getScoreLevel(85)).toBe('Excellent');
      expect(component.getScoreLevel(65)).toBe('Good');
      expect(component.getScoreLevel(45)).toBe('Needs Work');
      expect(component.getScoreLevel(undefined)).toBe('N/A');
    });
  });

  describe('Date formatting', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should format date string correctly', () => {
      const formatted = component.formatDate('2026-01-02');
      expect(formatted).toContain('2026');
    });

    it('should handle invalid date string', () => {
      const formatted = component.formatDate('invalid-date');
      expect(formatted).toBe('Invalid Date');
    });
  });

  describe('Edit functionality', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should start edit mode for a record', () => {
      component.startEdit(mockEvaluationRecords[0]);

      expect(component.editingId()).toBe(1);
      expect(component.editingForm()).toEqual({
        id: 1,
        score: 85,
        speedMetric: 45,
        accuracyMetric: 92,
        notes: 'Good progress',
      });
    });

    it('should handle null values when starting edit', () => {
      component.startEdit(mockEvaluationRecords[1]);

      expect(component.editingForm()?.score).toBe(78);
      expect(component.editingForm()?.speedMetric).toBeNull();
      expect(component.editingForm()?.notes).toBe('');
    });

    it('should cancel edit mode', () => {
      component.startEdit(mockEvaluationRecords[0]);
      component.cancelEdit();

      expect(component.editingId()).toBeNull();
      expect(component.editingForm()).toBeNull();
    });

    it('should update edit form field', () => {
      component.startEdit(mockEvaluationRecords[0]);
      component.updateEditForm('score', 90);

      expect(component.editingForm()?.score).toBe(90);
    });

    it('should update edit form field to null', () => {
      component.startEdit(mockEvaluationRecords[0]);
      component.updateEditForm('score', null);

      expect(component.editingForm()?.score).toBeNull();
    });
  });

  describe('Add functionality', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should show add form', () => {
      component.showAddEvaluationForm();

      expect(component.showAddForm()).toBe(true);
    });

    it('should cancel add form', () => {
      component.showAddEvaluationForm();
      component.newEvaluationForm.set({
        activityId: 1,
        evaluationDate: '2026-01-01',
        score: 80,
        speedMetric: null,
        accuracyMetric: null,
        notes: 'test',
      });
      component.cancelAdd();

      expect(component.showAddForm()).toBe(false);
      expect(component.newEvaluationForm().activityId).toBe(0);
    });

    it('should validate new form with required fields', () => {
      component.newEvaluationForm.set({
        activityId: 1,
        evaluationDate: '2026-01-01',
        score: null,
        speedMetric: null,
        accuracyMetric: null,
        notes: '',
      });

      expect(component.isNewFormValid()).toBe(true);

      component.newEvaluationForm.set({
        activityId: 0,
        evaluationDate: '',
        score: null,
        speedMetric: null,
        accuracyMetric: null,
        notes: '',
      });

      expect(component.isNewFormValid()).toBe(false);
    });
  });

  describe('Edit validation', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should validate edit form', () => {
      component.editingForm.set({
        id: 1,
        score: 80,
        speedMetric: null,
        accuracyMetric: null,
        notes: 'test',
      });

      expect(component.isEditFormValid()).toBe(true);
    });

    it('should invalidate null edit form', () => {
      component.editingForm.set(null);

      expect(component.isEditFormValid()).toBe(false);
    });
  });

  describe('Update new form', () => {
    beforeEach(() => {
      evaluationServiceSpy.getEvaluations.mockReturnValue(of(mockEvaluationRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should update new form field with number', () => {
      component.updateNewForm('score', 85);

      expect(component.newEvaluationForm().score).toBe(85);
    });

    it('should update new form field with null', () => {
      component.updateNewForm('score', null);

      expect(component.newEvaluationForm().score).toBeNull();
    });

    it('should update new form field with string', () => {
      component.updateNewForm('notes', 'test notes');

      expect(component.newEvaluationForm().notes).toBe('test notes');
    });
  });
});
