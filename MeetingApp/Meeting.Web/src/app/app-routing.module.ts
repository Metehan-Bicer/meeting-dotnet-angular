import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RegisterComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { MeetingListComponent } from './components/meeting-list/meeting-list.component';
import { MeetingCreateComponent } from './components/meeting-create/meeting-create.component';
import { MeetingDetailComponent } from './components/meeting-detail/meeting-detail.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: 'register', component: RegisterComponent, data: { animation: 'RegisterPage' } },
  { path: 'login', component: LoginComponent, data: { animation: 'LoginPage' } },
  { path: 'meetings', component: MeetingListComponent, canActivate: [AuthGuard], data: { animation: 'MeetingsPage' } },
  { path: 'meetings/create', component: MeetingCreateComponent, canActivate: [AuthGuard], data: { animation: 'CreateMeetingPage' } },
  { path: 'meetings/edit/:id', component: MeetingCreateComponent, canActivate: [AuthGuard], data: { animation: 'EditMeetingPage' } },
  { path: 'meetings/:id', component: MeetingDetailComponent, canActivate: [AuthGuard], data: { animation: 'MeetingDetailPage' } },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }