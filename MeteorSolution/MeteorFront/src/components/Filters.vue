<template>
  <div class="filters">
    <select v-model="filters.YearFrom">
      <option value="">Год с...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.YearTo">
      <option value="">Год по...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.Recclass">
      <option value="">Класс метеорита...</option>
      <option v-for="opt in recclassOptions" :key="opt.id" :value="opt.id">{{ opt.name }}</option>
    </select>

    <input
      type="text"
      v-model="filters.NameContains"
      placeholder="Часть названия..."
    />

    <select v-model="filters.SortBy">
      <option value="year">Год</option>
      <option value="count">Количество</option>
      <option value="mass">Суммарная масса</option>
    </select>

    <label>
      <input type="checkbox" v-model="filters.Desc" /> По убыванию
    </label>

    <button @click="applyFilters">Применить</button>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from "vue";
import { getMeteoriteTypes } from "../api/meteorites";

const emit = defineEmits(["apply"]);

const filters = reactive({
  YearFrom: "",
  YearTo: "",
  Recclass: "",
  NameContains: "",
  SortBy: "year",
  Desc: false,
});

const recclassOptions = ref([]);

onMounted(async () => {
  try {
    const types = await getMeteoriteTypes();
    recclassOptions.value = (types || []).map(t => ({ id: t.id ?? t.Id, name: t.name ?? t.Name }));
  } catch (e) {
    console.error('Failed to load meteorite types', e);
  }
});

function applyFilters() {
  const payload = {
    YearFrom: filters.YearFrom === "" ? undefined : Number(filters.YearFrom),
    YearTo: filters.YearTo === "" ? undefined : Number(filters.YearTo),
    Recclass: filters.Recclass === "" ? undefined : Number(filters.Recclass),
    NameContains: filters.NameContains || undefined,
    SortBy: filters.SortBy,
    Desc: filters.Desc,
  };
  emit('apply', payload);
}

const years = Array.from({ length: 2025 - 1800 }, (_, i) => 1800 + i);
</script>

<style scoped>
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}
</style>