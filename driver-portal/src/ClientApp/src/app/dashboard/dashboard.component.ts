import { Component, OnInit, ViewChild } from '@angular/core';
import { CaseManagementService } from '../shared/services/case-management/case-management.service';
import { Router } from '@angular/router';
import { MatAccordion } from '@angular/material/expansion';
import { ViewportScroller } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent {
  @ViewChild(MatAccordion) accordion!: MatAccordion;

  constructor(
    private caseManagementService: CaseManagementService,
    private router: Router,
    private viewportScroller: ViewportScroller
  ) {}

  public onClick(event: any, elementId: string): void {
    event.preventDefault();
    this.viewportScroller.scrollToAnchor(elementId);
  }
  // public  ngOnInit(): void {
  //     this.getRecentCase('F4B8B3');
  // }

  // getRecentCase(caseId: string){
  //   this.caseManagementService.getCaseById({caseId }).subscribe(
  //     (caseResponse) => {
  //       console.log(caseResponse);
  //     }
  //   );
  // }
}
