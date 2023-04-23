import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion } from '@angular/material/expansion';
import { Sort } from '@angular/material/sort';
import { Router } from '@angular/router';

import { Subscription } from 'rxjs';

import {
  CaseManagementService,
  DMERCase,
  DMERSearchCases,
} from '../../../../shared/services/case-management/case-management.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  // eslint-disable-next-line @typescript-eslint/explicit-member-accessibility
  busy!: Subscription;

  public dataSource: DMERCase[] = [];
  public filteredData: DMERCase[] = [];
  public showingDataInView: DMERCase[] = [];
  public searchBox = '';
  public prevSearchBox = '';
  public searchCasesInput = '';
  public selectedStatus = 'All Statuses';
  public pageNumber = 1;
  public pageSize = 10;
  public totalRecords = 0;
  public isLoading = true;
  public isShowResults = false;
  public searchedCase: DMERCase | null = {};

  // eslint-disable-next-line @typescript-eslint/explicit-member-accessibility
  @ViewChild(MatAccordion) accordion!: MatAccordion;

  // eslint-disable-next-line @typescript-eslint/explicit-member-accessibility
  statuses = [
    { label: 'All Statuses' },
    { label: 'In Progress' },
    { label: 'RSBC Received' },
    { label: 'Under RSBC Review' },
    { label: 'Decision Rendered' },
    { label: 'Cancelled/Closed' },
    { label: 'Transferred' },
  ];

  // eslint-disable-next-line @typescript-eslint/explicit-member-accessibility
  constructor(
    private caseManagementService: CaseManagementService,
    private router: Router
  ) {}

  public ngOnInit(): void {
    this.searchCases({ byStatus: ['All Statuses'] });
  }

  public search(): void {
    if (this.searchBox === '' || this.prevSearchBox === this.searchBox) return;
    // console.debug('search', this.searchBox);

    const searchParams = {
      byTitle: this.searchBox,
    };
    this.caseManagementService.getCases(searchParams).subscribe((cases) => {
      if (cases && Array.isArray(cases) && cases?.[0]) {
        this.searchedCase = cases[0];
        // console.log(this.searchedCase)
      } else {
        this.searchedCase = null;
      }

      this.prevSearchBox = this.searchBox;
      this.isShowResults = true;
    });
  }

  // eslint-disable-next-line @typescript-eslint/explicit-member-accessibility, @typescript-eslint/explicit-function-return-type
  closeResults() {
    this.searchBox = '';
    this.prevSearchBox = '';
    this.searchedCase = null;
    this.isShowResults = false;
  }

  onRowClick(event: any, row: any) {
    if (row.status == 'In Progress') {
      this.router.navigateByUrl('/cases/case/' + row.id);
    } else {
      // console.log("caseDetails", row);
      this.router.navigateByUrl('/caseDetails/' + row.id);
    }
  }

  searchCases(query?: any): void {
    let searchParams: DMERSearchCases = {
      ...query,
    };
    if (this.searchCasesInput?.length > 0) {
      searchParams['byPatientName'] = this.searchCasesInput;
      searchParams['byTitle'] = this.searchCasesInput;
    }

    if (this.selectedStatus?.length > 0) {
      searchParams['byStatus'] = [this.selectedStatus];
    }

    this.busy = this.caseManagementService
      .getCases(searchParams)
      .subscribe((cases) => {
        this.totalRecords = cases.length;
        this.pageNumber = 1;
        this.dataSource = cases;
        this.filteredData = cases;
        this.showingDataInView = this.dataSource.slice(0, this.pageSize);

        this.isLoading = false;
      });
  }

  filterLocally() {
    this.filteredData = this.dataSource.filter((item) => {
      if (
        this.selectedStatus !== 'All Statuses' &&
        item.status !== this.selectedStatus
      )
        return false;
      if (
        this.searchCasesInput?.length > 0 &&
        !item.title?.includes(this.searchCasesInput)
      )
        return false;
      return true;
    });

    this.totalRecords = this.filteredData.length;
    this.pageNumber = 1;

    this.showingDataInView = this.filteredData.slice(0, this.pageSize);
  }

  onStatusChanged() {
    this.filterLocally();
  }

  loadRecords() {
    if (this.pageNumber * this.pageSize > this.totalRecords) return;
    this.showingDataInView = this.filteredData.slice(
      0,
      this.pageSize * ++this.pageNumber
    );
  }

  clear() {
    this.searchCasesInput = '';
    this.selectedStatus = 'All Statuses';
    this.searchCases();
  }

  navigatetoDMER() {
    if (!this.searchedCase?.id) return;
    this.router.navigateByUrl('/cases/case/' + this.searchedCase?.id);
  }

  navigateToCaseDetails() {
    if (!this.searchedCase?.id) return;
    this.caseManagementService.selectedCase = this.searchedCase;
    this.router.navigateByUrl('/caseDetails/' + this.searchedCase?.id);
  }

  isScheduledAgeDmerType(dmerType: string) {
    return dmerType === 'Scheduled Age';
  }

  isCommercialDmerType(dmerType: string) {
    return dmerType === 'Commercial';
  }

  isKnownSuspectedDmerType(dmerType: string) {
    return dmerType === 'Known/Suspected Condition';
  }

  isSuspectedDmerType(dmerType: string) {
    return dmerType === 'Suspected Medical Condition';
  }

  sortData(sort: Sort) {
    const data = this.showingDataInView.slice();
    if (!sort.active || sort.direction === '') {
      this.showingDataInView = data;
      return;
    }

    this.showingDataInView = data.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      switch (sort.active) {
        case 'title':
          return compare(a.title, b.title, isAsc);
        case 'patientName':
          return compare(a.patientName, b.patientName, isAsc);
        case 'driverBirthDate':
          return compare(a.driverBirthDate, b.driverBirthDate, isAsc);
        case 'dmerType':
          return compare(a.dmerType, b.dmerType, isAsc);
        case 'modifiedOn':
          return compare(a.modifiedOn, b.modifiedOn, isAsc);
        case 'clinicName':
          return compare(a.clinicName, b.clinicName, isAsc);
        case 'status':
          return compare(a.status, b.status, isAsc);
        default:
          return 0;
      }
    });
  }
}

function compare(
  a: string | undefined | null,
  b: number | string | undefined | null,
  isAsc: boolean
) {
  // check for null or undefined
  if (a == null || b == null) {
    return 1;
  }
  return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}
