import { createRouter, createWebHistory } from 'vue-router'
import SearchView from '../views/SearchView.vue'
import ResultsView from '../views/ResultsView.vue'
import TrainDetailView from '../views/TrainDetailView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'search', component: SearchView },
    { path: '/results', name: 'results', component: ResultsView },
    { path: '/train', name: 'train-detail', component: TrainDetailView },
  ],
})
