document.addEventListener("DOMContentLoaded", () => {
    
    const searchBar = document.querySelector(".search-bar");
    const searchInput = document.getElementById("search-input");
    const searchButton = document.getElementById("search-button");
    const searchResults = document.getElementById("search-results");

    function generateSlug(str) {
        return str
            .replace(/đ/g, 'd')
            .replace(/Đ/g, 'd')
            .toLowerCase()
            .normalize("NFD").replace(/[\u0300-\u036f]/g, "")
            .replace(/[^a-z0-9\s-]/g, "")
            .replace(/\s+/g, "-") 
            .replace(/-+/g, "-")
            .replace(/^-+|-+$/g, "");
    }

    function performSearch() {
        const q = searchInput.value.trim();
        if (q.length >= 1) {
            window.location.href = `/tim-kiem?query=${encodeURIComponent(q)}`;
        }
    }

    searchButton.addEventListener("click", e => {
        e.preventDefault();
        if (!searchBar.classList.contains("active")) {
            searchBar.classList.add("active");
            searchInput.focus();
        } else {
            performSearch();
        }
    });

    searchInput.addEventListener("keypress", e => {
        if (e.key === "Enter") {
            e.preventDefault();
            performSearch();
        }
    });

    searchInput.addEventListener("input", () => {
        const q = searchInput.value.trim();
        if (q.length >= 1) {
            fetch(`/Search/SearchAjax?query=${encodeURIComponent(q)}`)
                .then(res => res.json())
                .then(data => {
                    searchResults.innerHTML = "";
                    if (data.success && data.data.length) {
                        data.data.forEach(comic => {
                            const item = document.createElement("div");
                            item.className = "result-item";
                            const slug = generateSlug(comic.comicName);
                            item.innerHTML = `
                                        <a href="/truyen-tranh/${slug}-${comic.comicId}">
                                          <img src="/${comic.comicImage}" alt="${comic.comicName}">
                                          <span>${comic.comicName}</span>
                                        </a>`;
                            searchResults.appendChild(item);
                        });
                        searchResults.style.display = "block";
                    } else {
                        searchResults.style.display = "none";
                    }
                });
        } else {
            searchResults.style.display = "none";
        }
    });

    
    const avatarContainer = document.querySelector('.user-avatar-container');
    const userMenu = document.querySelector('.user-menu');

    
    if (avatarContainer) {
        avatarContainer.addEventListener('click', (e) => {
            e.stopPropagation();
            userMenu.classList.toggle('active');
        });
    }

    
    document.addEventListener("click", (e) => {
        
        if (
            !searchBar.contains(e.target) &&
            !searchResults.contains(e.target)
        ) {
            searchResults.style.display = "none";
            searchBar.classList.remove("active");
        }

        
        if (
            userMenu &&
            avatarContainer &&
            !avatarContainer.contains(e.target) &&
            !userMenu.contains(e.target)
        ) {
            userMenu.classList.remove("active");
        }
    });
});