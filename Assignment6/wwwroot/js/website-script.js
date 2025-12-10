/* ============================================
   COMPLETE WEBSITE JAVASCRIPT
   Save as: wwwroot/js/website-script.js
   ============================================ */

$(document).ready(function () {
    if (localStorage.getItem('IsLoggedIn') == "true") {
        window.location.href = "/Login/Login";
    }

    // Initialize all functionality
    initializeNavigation();
    initializeWebsiteValidation();
    initializeAwardsCarousel();
});

/* ============================================
   NAVIGATION WITH SMOOTH SCROLL
   ============================================ */

function initializeNavigation() {
    const hamburger = document.querySelector('.hamburger-btn');
    const navMenu = document.querySelector('.nav-menu');
    const overlay = document.querySelector('.mobile-menu-overlay');
    const navLinks = document.querySelectorAll('.nav-link');
    const body = document.body;

    // Toggle mobile menu
    function toggleMenu() {
        hamburger.classList.toggle('active');
        navMenu.classList.toggle('active');
        overlay.classList.toggle('active');
        body.classList.toggle('menu-open');
    }

    // Close mobile menu
    function closeMenu() {
        hamburger.classList.remove('active');
        navMenu.classList.remove('active');
        overlay.classList.remove('active');
        body.classList.remove('menu-open');
    }

    // Hamburger click event
    if (hamburger) {
        hamburger.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleMenu();
        });
    }

    // Overlay click event
    if (overlay) {
        overlay.addEventListener('click', closeMenu);
    }

    // Nav links click event with smooth scroll
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');

            // Check if it's an anchor link (starts with #)
            if (href && href.startsWith('#')) {
                e.preventDefault();

                // Close menu on mobile
                if (window.innerWidth <= 768) {
                    closeMenu();
                }

                // Get target section
                const targetId = href;
                const targetSection = document.querySelector(targetId);

                if (targetSection) {
                    // Calculate offset for sticky header
                    const headerHeight = document.querySelector('.header').offsetHeight;
                    const targetPosition = targetSection.offsetTop - headerHeight;

                    // Smooth scroll to target with delay for menu close animation
                    setTimeout(() => {
                        window.scrollTo({
                            top: targetPosition,
                            behavior: 'smooth'
                        });
                    }, window.innerWidth <= 768 ? 300 : 0);
                }
            }
        });
    });

    // Close menu on window resize to desktop size
    window.addEventListener('resize', function () {
        if (window.innerWidth > 768 && navMenu.classList.contains('active')) {
            closeMenu();
        }
    });

    // Close menu on escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && navMenu.classList.contains('active')) {
            closeMenu();
        }
    });

    // Prevent menu closing when clicking inside menu
    if (navMenu) {
        navMenu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }
}

/* ============================================
   AWARDS CAROUSEL
   ============================================ */

let currentAwardSlide = 0;
const awardImages = document.querySelectorAll('.awards-carousel-images .award-image');
const totalAwardSlides = awardImages ? awardImages.length : 0;

function initializeAwardsCarousel() {
    if (totalAwardSlides === 0) return;

    // Create indicator dots
    const indicatorsContainer = document.getElementById('awardIndicators');
    if (indicatorsContainer) {
        for (let i = 0; i < totalAwardSlides; i++) {
            const dot = document.createElement('div');
            dot.className = `indicator-dot ${i === 0 ? 'active' : ''}`;
            dot.onclick = () => goToAwardSlide(i);
            indicatorsContainer.appendChild(dot);
        }
    }

    // Auto-advance slides every 5 seconds
    setInterval(() => {
        moveAwardSlide(1);
    }, 5000);
}

function moveAwardSlide(direction) {
    currentAwardSlide += direction;

    if (currentAwardSlide >= totalAwardSlides) {
        currentAwardSlide = 0;
    } else if (currentAwardSlide < 0) {
        currentAwardSlide = totalAwardSlides - 1;
    }

    updateAwardCarousel();
}

function goToAwardSlide(index) {
    currentAwardSlide = index;
    updateAwardCarousel();
}

function updateAwardCarousel() {
    const carouselImages = document.querySelector('.awards-carousel-images');
    if (carouselImages) {
        const offset = -currentAwardSlide * 100;
        carouselImages.style.transform = `translateX(${offset}%)`;
    }

    // Update indicators
    const indicators = document.querySelectorAll('.indicator-dot');
    indicators.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentAwardSlide);
    });
}

/* ============================================
   ROOM CARDS CAROUSEL
   ============================================ */

class Carousel {
    constructor(element) {
        this.card = element;
        this.container = element.querySelector('.carousel-images');
        this.images = element.querySelectorAll('.carousel-images img');
        this.currentIndex = 0;
        this.interval = null;
        this.isHovered = false;

        this.init();
    }

    init() {
        this.startAutoSlide();

        this.card.addEventListener('mouseenter', () => {
            this.isHovered = true;
            this.stopAutoSlide();
        });

        this.card.addEventListener('mouseleave', () => {
            this.isHovered = false;
            this.startAutoSlide();
        });
    }

    slide() {
        this.currentIndex = (this.currentIndex + 1) % this.images.length;
        const offset = -this.currentIndex * 100;
        this.container.style.transform = `translateX(${offset}%)`;
    }

    startAutoSlide() {
        if (this.interval) return;
        this.interval = setInterval(() => this.slide(), 2500);
    }

    stopAutoSlide() {
        if (this.interval) {
            clearInterval(this.interval);
            this.interval = null;
        }
    }
}

// Initialize all room carousels
document.addEventListener('DOMContentLoaded', () => {
    const carouselCards = document.querySelectorAll('.category-card');
    carouselCards.forEach(card => new Carousel(card));
});

/* ============================================
   CALENDAR INITIALIZATION
   ============================================ */

var calendarEl = document.getElementById('websiteCalendar');
var calendar;

function initializeCalendar() {
    if (calendar) {
        calendar.destroy();
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth'
        },
        height: 'auto',
        selectable: false,
        eventDisplay: 'none',
        dayMaxEvents: false,
        events: [],
        dayCellDidMount: function (info) {
            addBookingStatus(info.el, info.date);
        },
        datesSet: function () {
            setTimeout(() => {
                document.querySelectorAll('.fc-daygrid-day').forEach(dayEl => {
                    const dateStr = dayEl.getAttribute('data-date');
                    if (dateStr) {
                        const date = new Date(dateStr);
                        addBookingStatus(dayEl, date);
                    }
                });
            }, 100);
        }
    });

    calendar.render();
    loadBookingData();
}

function processBookingDataWithNames(bookings) {
    const dailyBookings = {};

    bookings.forEach((booking) => {
        const startDate = new Date(booking.dateFrom + 'T00:00:00');
        const endDate = new Date(booking.dateTo + 'T00:00:00');

        const currentDate = new Date(startDate);
        while (currentDate < endDate) {
            const dateKey = currentDate.toISOString().split('T')[0];

            if (!dailyBookings[dateKey]) {
                dailyBookings[dateKey] = [];
            }

            const existingBooking = dailyBookings[dateKey].find(b => b.id === booking.id);
            if (!existingBooking) {
                dailyBookings[dateKey].push({
                    id: booking.id,
                    homeId: booking.homeId,
                    homeName: booking.homeName
                });
            }

            currentDate.setDate(currentDate.getDate() + 1);
        }
    });

    return dailyBookings;
}

function loadBookingData() {
    console.log('=== LOADING BOOKING DATA ===');

    // Filter for confirmed bookings
    const confirmedBookings = window.allBookings.filter(booking => booking.isBooked === true);

    console.log('Total bookings:', window.allBookings.length);
    console.log('Confirmed bookings:', confirmedBookings.length);

    // Process and store
    window.bookingCalendarData = processBookingDataWithNames(confirmedBookings);
    updateCalendarDisplay();
}

function addBookingStatus(dayElement, date) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    date.setHours(0, 0, 0, 0);

    const isPastDate = date < today;

    if (!window.bookingCalendarData) {
        dayElement.classList.remove('booked', 'available', 'past-date');
        if (isPastDate) {
            dayElement.classList.add('past-date');
            dayElement.title = 'Past date';
        } else {
            dayElement.classList.add('available');
            dayElement.title = 'Available for booking';
        }
        return;
    }

    const dateKey = date.toISOString().split('T')[0];
    const dayBookings = window.bookingCalendarData[dateKey];

    dayElement.classList.remove('booked', 'available', 'past-date');

    // Remove existing booking info
    const existingInfo = dayElement.querySelector('.booking-info');
    if (existingInfo) {
        existingInfo.remove();
    }

    if (isPastDate) {
        dayElement.classList.add('past-date');
        dayElement.title = 'Past date';
        return;
    }

    if (dayBookings && dayBookings.length > 0) {
        dayElement.classList.add('booked');
        const propertyNames = [...new Set(dayBookings.map(b => b.homeName))];
        const tooltipText = `Booked Properties:\n${propertyNames.join('\n')}`;
        dayElement.title = tooltipText;

        // Add booking info badge
        if (propertyNames.length > 1) {
            const bookingInfo = document.createElement('div');
            bookingInfo.className = 'booking-info';
            bookingInfo.style.cssText = `
                position: absolute;
                top: 2px;
                left: 2px;
                background: rgba(255, 255, 255, 0.9);
                color: #495057;
                font-size: 10px;
                padding: 1px 4px;
                border-radius: 3px;
                font-weight: bold;
                z-index: 5;
            `;
            bookingInfo.textContent = `${propertyNames.length} properties`;
            dayElement.style.position = 'relative';
            dayElement.appendChild(bookingInfo);
        }
    } else {
        dayElement.classList.add('available');
        dayElement.title = 'Available for booking';
    }
}

function updateCalendarDisplay() {
    if (calendar) {
        calendar.render();
        setTimeout(() => {
            document.querySelectorAll('.fc-daygrid-day').forEach(dayEl => {
                const dateStr = dayEl.getAttribute('data-date');
                if (dateStr) {
                    const date = new Date(dateStr);
                    addBookingStatus(dayEl, date);
                }
            });
        }, 100);
    }
}

// Initialize calendar when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(initializeCalendar, 500);
});

/* ============================================
   BOOKING FORM FUNCTIONALITY
   ============================================ */

// Set minimum dates for date pickers
const today = new Date().toISOString().split('T')[0];
document.getElementById('checkin').setAttribute('min', today);
document.getElementById('checkout').setAttribute('min', today);

document.getElementById('checkin').addEventListener('change', function () {
    const checkinDate = this.value;
    document.getElementById('checkout').setAttribute('min', checkinDate);
});

function formatPrice(amount) {
    return new Intl.NumberFormat('en-IN', {
        style: 'currency',
        currency: 'INR',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

function searchAvailability() {
    const propertyId = document.getElementById('property').value;
    const checkin = document.getElementById('checkin').value;
    const checkout = document.getElementById('checkout').value;

    if (!propertyId || !checkin || !checkout) {
        showNotification('Please fill in all fields to search availability.', 'error');
        return;
    }

    if (!validateBookingDates()) {
        return;
    }

    $.ajax({
        url: '/Scheduler/CheckBookings',
        type: 'GET',
        data: {
            startDate: checkin,
            endDate: checkout,
            homeId: propertyId
        },
        success: function (data) {
            const homeName = getHomeName(propertyId);
            const dateRange = `${new Date(checkin).toLocaleDateString()} - ${new Date(checkout).toLocaleDateString()}`;

            if (data) {
                showNotification(
                    `${homeName} is already booked for ${dateRange}. Please select different dates.`,
                    "error"
                );
            } else {
                showNotification(
                    `${homeName} is available for ${dateRange}! You can proceed with booking.`,
                    "success"
                );
                showBookingOption(propertyId, checkin, checkout, homeName);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error checking availability:", error);
            showNotification("Error checking availability. Please try again.", "error");
        }
    });
}

function getHomeName(homeId) {
    const home = window.websiteData.homes.find(h => h.id == homeId);
    if (home) {
        return home.name;
    }

    const homeSelector = document.getElementById('property');
    for (let option of homeSelector.options) {
        if (option.value === homeId) {
            return option.text;
        }
    }
    return `Property ${homeId}`;
}

function showNotification(message, type = 'info', duration = 5000) {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => {
        notification.remove();
    });

    const iconMap = {
        'success': '✅',
        'error': '❌',
        'info': 'ℹ️',
        'warning': '⚠️'
    };

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <span class="notification-icon">${iconMap[type] || iconMap.info}</span>
        <span class="notification-message">${message}</span>
        <button class="notification-close" onclick="closeNotification(this)">&times;</button>
    `;

    document.body.appendChild(notification);

    if (duration > 0) {
        setTimeout(() => {
            closeNotification(notification.querySelector('.notification-close'));
        }, duration);
    }
}

function closeNotification(closeButton) {
    const notification = closeButton.parentElement;
    notification.classList.add('slide-out');
    setTimeout(() => {
        notification.remove();
    }, 300);
}

function showBookingOption(propertyId, checkin, checkout, homeName) {
    // Create and show booking button
    const existingActions = document.querySelector('.availability-actions');
    if (existingActions) {
        existingActions.remove();
    }
    const bookingActions = document.createElement('div');
    bookingActions.className = 'availability-actions';
    bookingActions.innerHTML = `
            <button class="book-now-btn" onclick="openBookingModal('${propertyId}', '${checkin}', '${checkout}', '${homeName}')">
                    <i class="fas fa-calendar-plus"></i> Book Your Stay
        </button>
        `;

    // Insert after the booking form
    document.querySelector('.booking-form').after(bookingActions);
}

function openBookingModal(propertyId, checkin, checkout, homeName) {
    // Populate modal with data
    document.getElementById('HomeId').value = propertyId;
    document.getElementById('BookingDateFrom').value = checkin;
    document.getElementById('BookingDateTo').value = checkout;
    document.getElementById('SelectedProperty').value = homeName;

    // Calculate days and pricing
    const checkInDate = new Date(checkin);
    const checkOutDate = new Date(checkout);
    const timeDiff = checkOutDate.getTime() - checkInDate.getTime();
    const totalDays = Math.ceil(timeDiff / (1000 * 3600 * 24));

    // Get pricing from model data
    const pricePerDay = getPriceForProperty(propertyId);
    const totalPrice = pricePerDay * totalDays;

    document.getElementById('TotalDays').value = totalDays;
    document.getElementById('TotalPrice').value = totalPrice;

    // Update summary display
    updateBookingSummary(homeName, checkin, checkout, totalDays, pricePerDay, totalPrice);

    // Reset room selection
    selectedRooms = [];
    localStorage.removeItem('selectedRoomData');
    updateRoomSelectionDisplay([], false);

    // Show modal
    const modal = document.getElementById('bookingModal');
    if (typeof bootstrap !== 'undefined') {
        const bookingModal = new bootstrap.Modal(modal);
        bookingModal.show();
    } else {
        modal.style.display = 'block';
        modal.classList.add('show');
    }
}

function validateBookingDates() {
    const checkinInput = document.getElementById('checkin');
    const checkoutInput = document.getElementById('checkout');

    if (!checkinInput.value || !checkoutInput.value) {
        return false;
    }

    const checkinDate = new Date(checkinInput.value);
    const checkoutDate = new Date(checkoutInput.value);
    const today = new Date();

    today.setHours(0, 0, 0, 0);
    checkinDate.setHours(0, 0, 0, 0);
    checkoutDate.setHours(0, 0, 0, 0);

    if (checkinDate < today) {
        showNotification('Check-in date cannot be in the past.', 'error');
        return false;
    }

    if (checkoutDate <= checkinDate) {
        showNotification('Check-out date must be after check-in date.', 'error');
        return false;
    }

    return true;
}

/* ============================================
   FORM VALIDATION (PLACEHOLDER)
   ============================================ */

function initializeWebsiteValidation() {
    // Add validation logic here if needed
    console.log('Validation initialized');
}
