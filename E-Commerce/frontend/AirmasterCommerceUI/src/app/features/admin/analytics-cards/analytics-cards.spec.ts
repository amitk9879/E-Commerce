import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnalyticsCards } from './analytics-cards';

describe('AnalyticsCards', () => {
  let component: AnalyticsCards;
  let fixture: ComponentFixture<AnalyticsCards>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnalyticsCards],
    }).compileComponents();

    fixture = TestBed.createComponent(AnalyticsCards);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
