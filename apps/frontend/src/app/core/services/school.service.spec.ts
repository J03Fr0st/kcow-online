import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SchoolService, School, ProblemDetails } from './school.service';
import { environment } from '../../../environments/environment';

describe('SchoolService', () => {
    let service: SchoolService;
    let httpMock: HttpTestingController;
    const apiUrl = environment.apiUrl;

    const mockSchools: School[] = [
        { id: 1, name: 'School A', isActive: true, printInvoice: false, importFlag: false },
        { id: 2, name: 'School B', isActive: false, printInvoice: false, importFlag: false },
    ];

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [SchoolService],
        });
        service = TestBed.inject(SchoolService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should fetch schools and update signal', (done) => {
        service.getSchools().subscribe((schools: School[]) => {
            expect(schools.length).toBe(2);
            expect(schools).toEqual(mockSchools);
            expect(service.schools()).toEqual(mockSchools);
            done();
        });

        const req = httpMock.expectOne(`${apiUrl}/schools`);
        expect(req.request.method).toBe('GET');
        expect(service.isLoading()).toBe(true);
        req.flush(mockSchools);
        expect(service.isLoading()).toBe(false);
    });

    it('should handle error during fetch and map to ProblemDetails', (done) => {
        service.getSchools().subscribe({
            error: (error: ProblemDetails) => {
                expect(error.title).toBe('Bad Request');
                expect(error.status).toBe(400);

                // Signal update might happen after subscribe callback in some RxJS/Signal combinations
                // or just need a tick to be sure
                setTimeout(() => {
                    expect(service.error()?.title).toBe('Bad Request');
                    done();
                }, 0);
            },
        });

        const req = httpMock.expectOne(`${apiUrl}/schools`);
        expect(service.isLoading()).toBe(true);
        req.flush({ title: 'Bad Request', status: 400 }, { status: 400, statusText: 'Bad Request' });
        expect(service.isLoading()).toBe(false);
    });

    it('should deactivate school and preserve fields', (done) => {
        const school: School = {
            id: 1,
            name: 'School A',
            address: '123 St',
            isActive: true,
            printInvoice: false,
            importFlag: false
        };

        service.deactivateSchool(1, school).subscribe((updated: School) => {
            expect(updated.isActive).toBe(false);
            expect(updated.address).toBe('123 St');
            done();
        });

        const req = httpMock.expectOne(`${apiUrl}/schools/1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body.isActive).toBe(false);
        expect(req.request.body.address).toBe('123 St');
        req.flush({ ...school, isActive: false });
    });

    it('should get active schools without side effects on global signal', (done) => {
        // First set some state in the signal
        service.getSchools().subscribe();
        httpMock.expectOne(`${apiUrl}/schools`).flush(mockSchools);
        expect(service.schools()).toEqual(mockSchools);

        // Now call getActiveSchools
        service.getActiveSchools().subscribe((active: School[]) => {
            expect(active.length).toBe(1);
            expect(active[0].id).toBe(1);
            // Global signal should NOT be affected by the filter
            expect(service.schools()).toEqual(mockSchools);
            done();
        });

        const req = httpMock.expectOne(`${apiUrl}/schools`);
        req.flush(mockSchools);
    });
});
