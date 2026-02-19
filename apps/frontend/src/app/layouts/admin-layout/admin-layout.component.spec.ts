import { Component } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AdminLayoutComponent } from './admin-layout.component';

@Component({ selector: 'app-sidebar', standalone: true, template: '' })
class MockSidebarComponent {}

@Component({ selector: 'app-navbar', standalone: true, template: '' })
class MockNavbarComponent {}

describe('AdminLayoutComponent', () => {
  let component: AdminLayoutComponent;
  let fixture: ComponentFixture<AdminLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminLayoutComponent],
      providers: [{ provide: ActivatedRoute, useValue: {} }],
    })
      .overrideComponent(AdminLayoutComponent, {
        remove: { imports: [SidebarComponent, NavbarComponent] },
        add: { imports: [MockSidebarComponent, MockNavbarComponent] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(AdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
