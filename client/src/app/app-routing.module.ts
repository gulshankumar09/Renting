import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { HomeComponent } from "./components/home/home.component";
import { PropertyListComponent } from "./components/property-list/property-list.component";
import { PropertyDetailComponent } from "./components/property-detail/property-detail.component";

const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "properties", component: PropertyListComponent },
  { path: "properties/:id", component: PropertyDetailComponent },
  {
    path: "auth",
    loadChildren: () =>
      import("./components/auth/auth.module").then((m) => m.AuthModule),
  },
  { path: "**", redirectTo: "" },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
