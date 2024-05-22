import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import {
  CUSTOM_ELEMENTS_SCHEMA,
  Component,
  OnInit,
  ViewChild,
  input,
} from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import {
  MatAccordion,
  MatExpansionModule,
  MatExpansionPanel,
} from '@angular/material/expansion';
import { CaseTypeComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/case-type/case-type.component';
import { CaseStatusComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/case-status/case-status.component';
import { DmerTypeComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/dmer-type/dmer-type.component';
import { DecisionOutcomeComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/decision-outcome/decision-outcome.component';
import { EligibleLicenseClassComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/eligible-license-class/eligible-license-class.component';
import { SubmissionTypeComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/submission-type/submission-type.component';
import { SubmissionStatusComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/submission-status/submission-status.component';
import { LetterTopicComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/letter-topic/letter-topic.component';
import { DmerStatusComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/case-definitions/dmer-status/dmer-status.component';
import { UploadDocumentComponent } from '../../../../shared-portal-ui/projects/core-ui/src/lib/upload-document/upload-document.component';
import { MatButtonModule } from '@angular/material/button';
import { PractitionerDMERList_SEED_DATA } from '../../seed-data/seed-data';
import { CaseDocument } from '../shared/api/models';
import { MatTabsModule } from '@angular/material/tabs';
import { CaseSubmissionsComponent } from '../../case-submissions/case-submissions.component';
import { SubmissionRequirementsComponent } from '../../submission-requirements/submission-requirements.component';

@Component({
  selector: 'app-case-details',
  standalone: true,
  imports: [
    MatCardModule,
    MatStepperModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatExpansionModule,
    CaseTypeComponent,
    CaseStatusComponent,
    DmerTypeComponent,
    DmerStatusComponent,
    DecisionOutcomeComponent,
    EligibleLicenseClassComponent,
    SubmissionTypeComponent,
    SubmissionStatusComponent,
    LetterTopicComponent,
    UploadDocumentComponent,
    MatTabsModule,
    CaseSubmissionsComponent,
    SubmissionRequirementsComponent
  ],
  templateUrl: './case-details.component.html',
  styleUrl: './case-details.component.scss',
  viewProviders: [MatExpansionPanel],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { displayDefaultIndicatorType: false },
    },
  ],
})
export class CaseDetailsComponent implements OnInit {
  caseId = input();
  caseDetails: CaseDocument | undefined;

  @ViewChild(MatAccordion) accordion!: MatAccordion;
  selectedIndex = 0;
  @ViewChild('stepper') stepper!: MatStepper;

  constructor(private breakpointObserver: BreakpointObserver) {}

  ngAfterViewInit(): void {
    this.breakpointObserver
      .observe(['(max-width: 768px)'])
      .subscribe((result) => {
        if (result.matches) {
          // this.stepper._stepsList.toArray()[this.selectedIndex].expanded = true;
          this.stepper.orientation = 'vertical';
        } else {
          this.stepper.orientation = 'horizontal';
        }
      });
  }

  public ngOnInit(): void {
    console.log(this.caseId());
    this.caseDetails = PractitionerDMERList_SEED_DATA.find(
      (c) => c.caseNumber === this.caseId()
    );
    // this.caseManagementService
    //   .getMostRecentCase(this.loginService.userProfile?.id as string)
    //   .subscribe((recentCase) => {
    //     this.caseDetails = recentCase;
    //     if (recentCase.status === 'Opened') {
    //       this.selectedIndex = 0;
    //     }
    //     if (recentCase.status === 'Open Pending Submission') {
    //       this.selectedIndex = 1;
    //     }
    //     if (recentCase.status === 'Under Review') {
    //       this.selectedIndex = 2;
    //     }
    //     if (recentCase.status === 'File End Tasks') {
    //       this.selectedIndex = 3;
    //     }
    //     if (recentCase.status === 'Intake Validation') {
    //       this.selectedIndex = 4;
    //     }
    //     if (recentCase.status === 'Closed') {
    //       this.selectedIndex = 5;
    //     }
    //   });
  }
}
