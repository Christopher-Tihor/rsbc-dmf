import { Routes } from '@angular/router';

import { DriverSearchComponent } from './driver-search/driver-search.component';
import { SearchComponent } from './search/search.component';
import { RemedialComponent } from './remedial/remedial.component';
import { AssistDriverComponent } from './assist-driver/assist-driver.component';

export const routes: Routes = [
  { path: '', redirectTo: 'search', pathMatch: 'full' },

  {
    path: '',

    children: [
      { path: 'search', component: SearchComponent },
      { path: 'remedial', component: RemedialComponent },
      { path: 'driverSearch', component: DriverSearchComponent },
      { path: 'assistDriver', component: AssistDriverComponent },
    ],
  },
];
